namespace MiniMq.WebApi.Routing
{
    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    using MiniMQ.Core.Core;
    using MiniMQ.Core.Message;
    using MiniMQ.Core.MessageHandlers.General;
    using MiniMQ.Core.Routing;
    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    public class Router
    {
        private readonly IMessageHandlerContainer messageHandlerContainer;

        private readonly IMessageHandlerProducer messageHandlerProducer;

        public Router(IMessageHandlerContainer messageHandlerContainer, IMessageHandlerProducer messageHandlerProducer, IWebSocketConnector webSocketConnector)
        {
            this.messageHandlerContainer = messageHandlerContainer;
            this.messageHandlerProducer = messageHandlerProducer;
            this.webSocketConnector = webSocketConnector;
        }

        public static readonly Task<RouteResult> RouteFailedTask = Task.FromResult(new RouteResult { Description = "Routing failed" });

        public struct RouteResult
        {
            public bool Failed { get; set; }

            public RouteResult(string description, bool failed = true)
            {
                this.Description = description;
                this.Failed = failed;
            }

            public string Description { get; set; }
        }

        private static readonly PathActionParser PathActionParser = new PathActionParser(PathActionMap.Items);

        private IWebSocketConnector webSocketConnector;

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
            var pathAction = PathActionParser.GetPathAction(path);
            var messageHandlerName = GetNextPathParameter(path, pathAction.Path.Length);

            if (pathAction.PathAction == PathAction.WebSocketConnect)
            {
                return this.ConnectWebSocketRequestValidation(context, messageHandlerName, GetPathRequestInformation(path, pathAction, messageHandlerName));
            }

            var pipeline = new HttpContextResponseOutputMessagePipeline(context);

            switch (pathAction.PathAction)
            {
                case PathAction.SendMessage:
                    return this.SendMessageAsync(context, path, messageHandlerName, pathAction);

                case PathAction.ReceiveMessage:
                    return this.ProcessGet(messageHandlerName, pipeline);

                case PathAction.ReceiveMessageWait:
                    return this.ProcessGetAwait(messageHandlerName, new ContextIsClientConnected(context), pipeline);

                case PathAction.SendAndReceiveMessageWait:
                    return this.SendAndReceiveMessageWaitAsync(context, path, messageHandlerName, pipeline, pathAction);

                case PathAction.CreateQueue:
                    return this.CreateQueue(messageHandlerName);

                case PathAction.CreateApplication:
                    return this.CreateApplication(messageHandlerName);

                case PathAction.CreateBus:
                    return this.CreateBus(messageHandlerName);
            }

            return RouteFailedTask;
        }

        private Task SendAndReceiveMessageWaitAsync(HttpContext context, string path, string messageHandlerName, HttpContextResponseOutputMessagePipeline pipeline, PathActionMapItem pathAction)
        {
            {
                var method = context.Request.Method;
                var isPost = string.CompareOrdinal("POST", method) == 0;

                return isPost
                           ? this.SendAndReceiveMessageWait(messageHandlerName, context.Request.Body, new ContextIsClientConnected(context), pipeline)
                           : this.SendAndReceiveMessageWait(messageHandlerName, GetPathRequestInformation(path, pathAction, messageHandlerName), new ContextIsClientConnected(context), pipeline);
            }
        }

        private Task SendMessageAsync(HttpContext context, string path, string messageHandlerName, PathActionMapItem pathAction)
        {
            var method = context.Request.Method;
            var isPost = string.CompareOrdinal("POST", method) == 0;
            return isPost ? this.ProcessAdd(messageHandlerName, context.Request.Headers, context.Request.Body) : this.ProcessAddSimple(messageHandlerName, GetPathRequestInformation(path, pathAction, messageHandlerName));
        }

        private static string GetPathRequestInformation(string path, PathActionMapItem pathAction, string messageHandlerName)
        {
            var startIndex = pathAction.Path.Length + messageHandlerName.Length + 1;

            if (startIndex >= path.Length)
            {
                return string.Empty;
            }

            return path.Substring(startIndex);
        }

        private Task<RouteResult> ConnectWebSocketRequestValidation(HttpContext context, string messageHandlerName, string requestInformation)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.WriteAsync("Attempt to call web sockets connect without being a web sockets request");
                return RouteFailedTask;
            }

            var messageHandler = this.GetMessageHandler(messageHandlerName);

            if (messageHandler == null)
            {
                return Task.FromResult(new RouteResult("Message handler does not exist"));
            }

            if (messageHandler.SupportsWebSocketConnections == false)
            {
                return Task.FromResult(new RouteResult("Message handler does not support web socket connections"));
            }

            return this.ConnectWebSocket(context, messageHandler, requestInformation);
        }

        private async Task<RouteResult> ConnectWebSocket(HttpContext context, IMessageHandler messageHandler, string requestInformation)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            if (webSocket == null)
            {
                // Not sure someone will ever receive this..
                return new RouteResult("AcceptWebSocketAsync returned null");
            }

            if (webSocket.State == WebSocketState.Open)
            {
                await this.webSocketConnector.ConnectAsync(webSocket, messageHandler);

//                var socketRouter = new WebSocketClientRouter(webSocket, messageHandler);
//                await socketRouter.ServeClientAsync();

                return new RouteResult();
            }

            // Not sure someone will ever receive this..
            return new RouteResult("Web socket is not open");

        }

        private async Task SendAndReceiveMessageWait(string messageHandlerName, string inputText, IClientConnected clientConnected, IMessagePipeline pipeline)
        {
            var messageHandler = this.GetMessageHandler(messageHandlerName);

            if (messageHandler != null)
            {
                var factory = messageHandler.MessageFactory;
                var messageToSend = await factory.CreateMessage(inputText);

                await this.SendAndReceiveAwait(messageHandlerName, messageToSend, clientConnected, pipeline).ConfigureAwait(false);
            }
        }

        private IMessageHandler GetMessageHandler(string messageHandlerName)
        {
            var messageHandler = this.messageHandlerContainer.GetMessageHandler(messageHandlerName);
            return messageHandler;
        }

        private async Task SendAndReceiveMessageWait(string messageHandlerName, Stream inputStream, IClientConnected clientConnected, IMessagePipeline pipeline)
        {
            var messageHandler = this.GetMessageHandler(messageHandlerName);

            if (messageHandler != null)
            {
                var factory = messageHandler.MessageFactory;
                var messageToSend = await factory.CreateMessage(inputStream).ConfigureAwait(false);

                await this.SendAndReceiveAwait(messageHandlerName, messageToSend, clientConnected, pipeline).ConfigureAwait(false);
            }
        }

        private async Task CreateQueue(string messageHandlerName)
        {
            this.messageHandlerContainer.AddMessageHandler(
                messageHandlerName,
                await this.messageHandlerProducer.QueueFactory.Create(messageHandlerName).ConfigureAwait(false));
        }

        private async Task CreateBus(string busName)
        {
            this.messageHandlerContainer.AddMessageHandler(
                busName,
                await this.messageHandlerProducer.BusFactory.Create(busName));
        }

        private async Task CreateApplication(string applicationName)
        {
            this.messageHandlerContainer.AddMessageHandler(
                applicationName,
                await this.messageHandlerProducer.ApplicationFactory.Create(applicationName).ConfigureAwait(false));
        }

        private async Task ProcessAddSimple(string messageHandlerName, string message)
        {
            var messageHandler = this.GetMessageHandler(messageHandlerName);

            if (messageHandler != null)
            {
                var messageFactory = messageHandler.MessageFactory;
                await messageHandler.SendMessageAsync(await messageFactory.CreateMessage(message).ConfigureAwait(false)).ConfigureAwait(false);
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
            var messageHandler = this.GetMessageHandler(messageHandlerName);
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
            var messageHandler = this.GetMessageHandler(messageHandlerName);
            var message = messageHandler.ReceiveMessageOrNull();

            if (message != null)
            {
                return pipeline.SendMessageAsync(message);
            }

            return Task.CompletedTask;
        }

        private async Task ProcessAdd(string messageHandlerName, IHeaderDictionary headers, Stream inputStream)
        {
            var messageHandler = this.GetMessageHandler(messageHandlerName);

            if (messageHandler != null)
            {
                var messageFactory = messageHandler.MessageFactory;

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

                await messageHandler.SendMessageAsync(message).ConfigureAwait(false);
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