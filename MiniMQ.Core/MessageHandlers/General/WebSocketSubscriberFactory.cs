namespace MiniMQ.Core.MessageHandlers.General
{
    using MiniMQ.Core.MessageHandlers.InMemory.Queue;
    using MiniMQ.Model.Core.MessageHandler;

    public class WebSocketSubscriberFactory : IWebSocketSubscriberFactory
    {
        private readonly WebSubscriberSettings settings;

        public WebSocketSubscriberFactory(WebSubscriberSettings settings)
        {
            this.settings = settings;
        }

        public IWebSocketSubscriber CreateSubscriber(IMessageHandler messageHandler, IWebSocketClient webSocketClient)
        {
            return new WebSocketSubscriber(
                messageHandler, 
                webSocketClient, 
                new WebSocketDestinationMessagePipeline(messageHandler, webSocketClient),
                this.settings);
        }


    }
}