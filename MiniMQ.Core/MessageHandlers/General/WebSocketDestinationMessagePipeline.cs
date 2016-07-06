namespace MiniMQ.Core.MessageHandlers.General
{
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    internal struct WebSocketDestinationMessagePipeline: IMessagePipeline
    {
        private readonly IWebSocketClient webSocketClient;
        private readonly IMessageHandler messageHandler;

        public WebSocketDestinationMessagePipeline(IMessageHandler messageHandler, IWebSocketClient webSocketClient)
        {
            this.messageHandler = messageHandler;
            this.webSocketClient = webSocketClient;
        }

        public Task SendMessageAsync(IMessage message)
        {
            if (this.webSocketClient.IsConnected)
            {
                return this.webSocketClient.SendMessageAsync(message, CancellationToken.None);
            }

            // Client is no longer connected - post the message back to the queue
            return this.messageHandler.SendMessageAsync(message);
        }
    }
}