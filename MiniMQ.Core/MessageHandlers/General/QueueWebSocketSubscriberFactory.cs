namespace MiniMQ.Core.MessageHandlers.General
{
    using MiniMQ.Core.MessageHandlers.InMemory.Queue;
    using MiniMQ.Model.Core.MessageHandler;

    public class QueueWebSocketSubscriberFactory : IWebSocketSubscriberFactory
    {
        private readonly WebSubscriberSettings settings;

        public QueueWebSocketSubscriberFactory(WebSubscriberSettings settings)
        {
            this.settings = settings;
        }

        public IWebSocketSubscriber CreateSubscriber(IMessageHandler messageHandler, IWebSocketClient webSocketClient)
        {
            return new QueueWebSocketSubscriber(
                messageHandler, 
                webSocketClient, 
                new WebSocketDestinationMessagePipeline(messageHandler, webSocketClient),
                this.settings);
        }


    }
}