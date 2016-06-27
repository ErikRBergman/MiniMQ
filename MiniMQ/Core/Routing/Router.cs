namespace MiniMQ.Core.Routing
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    using MessageHandler;

    using Message;

    public class Router
    {
        private readonly IMessageHandlerContainer messageHandlerContainer;

        private readonly IMessageHandlerFactory messageHandlerFactory;

        public Router(
            IMessageHandlerContainer messageHandlerContainer,
            IMessageHandlerFactory messageHandlerFactory)
        {
            this.messageHandlerContainer = messageHandlerContainer;
            this.messageHandlerFactory = messageHandlerFactory;
        }

        public static readonly Task RouteFailedTask = Task.FromResult("Routing failed");

        private static readonly PathActionParser PathActionParser = new PathActionParser(PathActionMap.Items);

        private struct ContextIsClientConnected : IClientConnected
        {
            private readonly HttpContext context;

            public ContextIsClientConnected(HttpContext context)
            {
                this.context = context;
            }

            public bool IsClientConnected => this.context.Response.IsClientConnected;
        }

        public Task RouteCall(HttpContext context, string path)
        {
            if (path.Length >= 5)
            {
                var method = context.Request.HttpMethod;

                var pathAction = PathActionParser.GetPathAction(path);
                var messageHandlerName = GetNextPathParameter(path, pathAction.Path.Length);

                var isPost = string.CompareOrdinal("POST", method) == 0;

                var pipeline = new HttpContextResponseOutputMessagePipeline(context.Response);

                switch (pathAction.PathAction)
                {
                    case PathAction.SendMessage:
                            return isPost ? 
                                this.ProcessAdd(messageHandlerName, context.Request.Headers, context.Request.InputStream) : 
                                this.ProcessAddSimple(messageHandlerName, context.Request.Headers, path.Substring(pathAction.Path.Length + messageHandlerName.Length + 1));

                    case PathAction.ReceiveMessage:
                            return this.ProcessGet(messageHandlerName, pipeline);

                    case PathAction.ReceiveMessageWait:
                            return this.ProcessGetAwait(messageHandlerName, new ContextIsClientConnected(context), pipeline);
                        
                    case PathAction.SendAndReceiveMessageWait:
                        return
                            isPost ? 
                                this.SendAndReceiveMessageWait(messageHandlerName, context.Request.InputStream, new ContextIsClientConnected(context), pipeline) : 
                                this.SendAndReceiveMessageWait(messageHandlerName, path.Substring(pathAction.Path.Length + messageHandlerName.Length + 1), new ContextIsClientConnected(context), pipeline);

                    case PathAction.CreateQueue:
                        return this.CreateQueue(messageHandlerName, context.Request.Headers);

                    case PathAction.CreateApplication:
                        return this.CreateApplication(messageHandlerName, context.Request.Headers);

                    case PathAction.CreateBus:
                        return this.CreateBus(messageHandlerName, context.Request.Headers);
                }
            }

            return RouteFailedTask;
        }

        private async Task SendAndReceiveMessageWait(string messageHandlerName, string inputText, IClientConnected clientConnected, IMessagePipeline pipeline)
        {
            var messageHandler = this.messageHandlerContainer.GetMessageHandler(messageHandlerName);

            if (messageHandler != null)
            {
                var factory = messageHandler.GetMessageFactory();
                var messageToSend = await factory.CreateMessage(inputText);

                await SendAndReceiveAwait(messageHandlerName, messageToSend, clientConnected, pipeline).ConfigureAwait(false);
            }
        }

        private async Task SendAndReceiveMessageWait(string messageHandlerName, Stream inputStream, IClientConnected clientConnected, IMessagePipeline pipeline)
        {
            var messageHandler = this.messageHandlerContainer.GetMessageHandler(messageHandlerName);

            if (messageHandler != null)
            {
                var factory = messageHandler.GetMessageFactory();
                var messageToSend = await factory.CreateMessage(inputStream).ConfigureAwait(false);

                await SendAndReceiveAwait(messageHandlerName, messageToSend, clientConnected, pipeline).ConfigureAwait(false);
            }
        }

        private async Task CreateQueue(string messageHandlerName, NameValueCollection headers)
        {
            this.messageHandlerContainer.AddMessageHandler(
                messageHandlerName,
                await this.messageHandlerFactory.CreateQueue(messageHandlerName).ConfigureAwait(false));
        }

        private async Task CreateBus(string busName, NameValueCollection headers)
        {
            this.messageHandlerContainer.AddMessageHandler(
                busName,
                await this.messageHandlerFactory.CreateBus(busName));
        }

        private async Task CreateApplication(string applicationName, NameValueCollection headers)
        {
            this.messageHandlerContainer.AddMessageHandler(
                applicationName,
                await this.messageHandlerFactory.CreateApplication(applicationName).ConfigureAwait(false));
        }

        private async Task ProcessAddSimple(string messageHandlerName, NameValueCollection headers, string message)
        {
            var messageHandler = this.messageHandlerContainer.GetMessageHandler(messageHandlerName);

            if (messageHandler != null)
            {
                var messageFactory = messageHandler.GetMessageFactory();
                await messageHandler.SendMessage(await messageFactory.CreateMessage(message).ConfigureAwait(false)).ConfigureAwait(false);
                return;
            }

            throw new FileNotFoundException("Message handler " + messageHandlerName + " does not exist");
        }

        private Task ProcessGetAwait(string messageHandlerName, IClientConnected clientConnected, IMessagePipeline pipeline)
        {
            return SendAndReceiveAwait(messageHandlerName, null, clientConnected, pipeline);
        }

        private Task SendAndReceiveAwait(string messageHandlerName, IMessage messageToSend, IClientConnected clientConnected, IMessagePipeline pipeline)
        {
            var messageHandler = this.messageHandlerContainer.GetMessageHandler(messageHandlerName);
            return SendAndReceiveAwait(messageHandler, messageToSend, clientConnected, pipeline);
        }

        private static Task SendAndReceiveAwait(IMessageHandler messageHandler, IMessage messageToSend, IClientConnected clientConnected, IMessagePipeline pipeline)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            Task<IMessage> getNextMessageTask;

            if (messageToSend == null)
            {
                getNextMessageTask = messageHandler.ReceiveMessageAsync(cancellationTokenSource.Token);
            }
            else
            {
                getNextMessageTask = messageHandler.SendAndReceiveMessageAsync(messageToSend, cancellationTokenSource.Token);
            }

            if (getNextMessageTask.IsCompleted)
            {
                if (clientConnected.IsClientConnected == false)
                {
                    return Task.CompletedTask;
                }

                return SendMessageToPipeline(pipeline, getNextMessageTask);
            }

            return SendAndReceiveAwaitWait(getNextMessageTask, clientConnected, pipeline, cancellationTokenSource);
        }

        private static async Task SendAndReceiveAwaitWait(Task<IMessage> getNextMessageTask, IClientConnected clientConnected, IMessagePipeline pipeline, CancellationTokenSource cancellationTokenSource)
        {
            while (true)
            {
                var doneTask = await Task.WhenAny(getNextMessageTask, Task.Delay(TimeSpan.FromSeconds(10), CancellationToken.None)).ConfigureAwait(false);

                // If connection was dropped while waiting
                if (clientConnected.IsClientConnected == false)
                {
                    cancellationTokenSource.Cancel();
                    return;
                }

                if (doneTask == getNextMessageTask)
                {
                    await SendMessageToPipeline(pipeline, getNextMessageTask);
                    return;
                }
            }
        }

        private static async Task SendMessageToPipeline(IMessagePipeline pipeline, Task<IMessage> getNextMessageTask)
        {
            var message = getNextMessageTask.Result;

            if (message != null)
            {
                await pipeline.SendMessage(message).ConfigureAwait(false);
            }
        }

        private Task ProcessGet(string messageHandlerName, IMessagePipeline pipeline)
        {
            var messageHandler = this.messageHandlerContainer.GetMessageHandler(messageHandlerName);
            var message = messageHandler.ReceiveMessageOrNull();

            if (message != null)
            {
                return pipeline.SendMessage(message);
            }

            return Task.CompletedTask;
        }

        private async Task ProcessAdd(string messageHandlerName, NameValueCollection headers, Stream inputStream)
        {
            var messageHandler = this.messageHandlerContainer.GetMessageHandler(messageHandlerName);

            if (messageHandler != null)
            {
                var messageFactory = messageHandler.GetMessageFactory();

                var transactionId = headers["transactionId"];

                IMessage message;

                if (transactionId != null)
                {
                    message = await messageFactory.CreateMessage(inputStream, transactionId).ConfigureAwait(false);
                }
                else
                {
                    message = await messageFactory.CreateMessage(inputStream).ConfigureAwait(false);
                }

                await messageHandler.SendMessage(message).ConfigureAwait(false);
                return;
            }

            throw new FileNotFoundException("Queue " + messageHandlerName + " does not exist");
        }

        private static string GetNextPathParameter(string path, int position)
        {
            var separatorPosition = path.IndexOf('/', position);

            if (separatorPosition == -1)
            {
                return path.Substring(position);
            }

            return path.Substring(position, separatorPosition - position);
        }


    }
}