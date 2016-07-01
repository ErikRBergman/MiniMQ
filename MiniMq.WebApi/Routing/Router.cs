namespace MiniMq.WebApi.Routing
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    using MiniMQ.Core.Core;
    using MiniMQ.Core.Message;
    using MiniMQ.Core.MessageHandler;
    using MiniMQ.Core.Routing;

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
                this.IsClientConnected = true;
                this.context.RequestAborted.Register(this.AbortCallback);
            }

            private void AbortCallback()
            {
                this.IsClientConnected = false;
            }

            public bool IsClientConnected { get; private set; }
        }

        public Task RouteCall(HttpContext context, string path)
        {
            if (path.Length >= 5)
            {
                var method = context.Request.Method;

                var pathAction = PathActionParser.GetPathAction(path);
                var messageHandlerName = GetNextPathParameter(path, pathAction.Path.Length);


                var pipeline = new HttpContextResponseOutputMessagePipeline(context);

                switch (pathAction.PathAction)
                {
                    case PathAction.SendMessage:
                        {
                            var isPost = string.CompareOrdinal("POST", method) == 0;
                            return isPost ?
                                    this.ProcessAdd(messageHandlerName, context.Request.Headers, context.Request.Body) :
                                    this.ProcessAddSimple(messageHandlerName, path.Substring(pathAction.Path.Length + messageHandlerName.Length + 1));
                        }

                    case PathAction.ReceiveMessage:
                        return this.ProcessGet(messageHandlerName, pipeline);

                    case PathAction.ReceiveMessageWait:
                        return this.ProcessGetAwait(messageHandlerName, new ContextIsClientConnected(context), pipeline);

                    case PathAction.SendAndReceiveMessageWait:
                        {
                            var isPost = string.CompareOrdinal("POST", method) == 0;

                            return isPost ?
                                this.SendAndReceiveMessageWait(messageHandlerName, context.Request.Body, new ContextIsClientConnected(context), pipeline) :
                                this.SendAndReceiveMessageWait(messageHandlerName, path.Substring(pathAction.Path.Length + messageHandlerName.Length + 1), new ContextIsClientConnected(context), pipeline);
                        }

                    case PathAction.CreateQueue:
                        return this.CreateQueue(messageHandlerName);

                    case PathAction.CreateApplication:
                        return this.CreateApplication(messageHandlerName);

                    case PathAction.CreateBus:
                        return this.CreateBus(messageHandlerName);
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

                await this.SendAndReceiveAwait(messageHandlerName, messageToSend, clientConnected, pipeline).ConfigureAwait(false);
            }
        }

        private async Task SendAndReceiveMessageWait(string messageHandlerName, Stream inputStream, IClientConnected clientConnected, IMessagePipeline pipeline)
        {
            var messageHandler = this.messageHandlerContainer.GetMessageHandler(messageHandlerName);

            if (messageHandler != null)
            {
                var factory = messageHandler.GetMessageFactory();
                var messageToSend = await factory.CreateMessage(inputStream).ConfigureAwait(false);

                await this.SendAndReceiveAwait(messageHandlerName, messageToSend, clientConnected, pipeline).ConfigureAwait(false);
            }
        }

        private async Task CreateQueue(string messageHandlerName)
        {
            this.messageHandlerContainer.AddMessageHandler(
                messageHandlerName,
                await this.messageHandlerFactory.CreateQueue(messageHandlerName).ConfigureAwait(false));
        }

        private async Task CreateBus(string busName)
        {
            this.messageHandlerContainer.AddMessageHandler(
                busName,
                await this.messageHandlerFactory.CreateBus(busName));
        }

        private async Task CreateApplication(string applicationName)
        {
            this.messageHandlerContainer.AddMessageHandler(
                applicationName,
                await this.messageHandlerFactory.CreateApplication(applicationName).ConfigureAwait(false));
        }

        private async Task ProcessAddSimple(string messageHandlerName, string message)
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
            return this.SendAndReceiveAwait(messageHandlerName, null, clientConnected, pipeline);
        }

        private Task SendAndReceiveAwait(string messageHandlerName, IMessage messageToSend, IClientConnected clientConnected, IMessagePipeline pipeline)
        {
            var messageHandler = this.messageHandlerContainer.GetMessageHandler(messageHandlerName);
            return SendAndReceiveAwait(messageHandler, messageToSend, clientConnected, pipeline);
        }

        private static Task SendAndReceiveAwait(IMessageHandler messageHandler, IMessage messageToSend, IClientConnected clientConnected, IMessagePipeline pipeline)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            Task getNextMessageTask;

            if (messageToSend == null)
            {
                getNextMessageTask = messageHandler.ReceiveMessageAsync(pipeline, cancellationTokenSource.Token);
            }
            else
            {
                getNextMessageTask = messageHandler.SendAndReceiveMessageAsync(messageToSend, pipeline, cancellationTokenSource.Token);
            }

            if (getNextMessageTask.IsCompleted)
            {
                return Task.CompletedTask;
            }

            return SendAndReceiveAwaitWait(getNextMessageTask, clientConnected, pipeline, cancellationTokenSource);
        }

        private static async Task SendAndReceiveAwaitWait(Task getNextMessageTask, IClientConnected clientConnected, IMessagePipeline pipeline, CancellationTokenSource cancellationTokenSource)
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
                    return;
                }
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

        private async Task ProcessAdd(string messageHandlerName, IHeaderDictionary headers, Stream inputStream)
        {
            var messageHandler = this.messageHandlerContainer.GetMessageHandler(messageHandlerName);

            if (messageHandler != null)
            {
                var messageFactory = messageHandler.GetMessageFactory();

                var transactionId = headers["transactionId"];

                IMessage message;

                if (transactionId.Count != 0)
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