namespace MiniMQ.Core.MessageHandlers.General
{
    using MiniMQ.Core.MessageHandlers.InMemory.Queue;
    using MiniMQ.Model.Core.MessageHandler;

    public class ApplicationWebSocketSubscriberFactory : IWebSocketSubscriberFactory
    {
        private readonly WebSubscriberSettings settings;

        public ApplicationWebSocketSubscriberFactory(WebSubscriberSettings settings)
        {
            this.settings = settings;
        }

        public IWebSocketSubscriber CreateSubscriber(IMessageHandler messageHandler, IWebSocketClient webSocketClient)
        {
            return new ApplicationWebSocketSubscriber(
                messageHandler, 
                webSocketClient, 
                new WebSocketDestinationMessagePipeline(messageHandler, webSocketClient),
                this.settings);
        }


    }
}