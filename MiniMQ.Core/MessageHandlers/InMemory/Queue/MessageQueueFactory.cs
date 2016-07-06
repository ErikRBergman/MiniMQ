namespace MiniMQ.Core.MessageHandlers.InMemory.Queue
{
    using System.Threading.Tasks;

    using MiniMQ.Core.MessageHandlers.General;
    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    public class MessageQueueFactory : IMessageHandlerFactory
    {
        private readonly IMessageFactory applicationMessageHandlerFactory;

        private readonly IWebSocketSubscriberFactory webSocketSubscriberFactory;

        public MessageQueueFactory(IMessageFactory applicationMessageHandlerFactory, IWebSocketSubscriberFactory webSocketSubscriberFactory)
        {
            this.applicationMessageHandlerFactory = applicationMessageHandlerFactory;
            this.webSocketSubscriberFactory = webSocketSubscriberFactory;
        }

        public Task<IMessageHandler> Create(string applicationName)
        {
            return Task.FromResult(
                (IMessageHandler)new InMemoryMessageQueue(
                    applicationName, 
                    this.applicationMessageHandlerFactory, 
                    this.webSocketSubscriberFactory));
        }
    }
}
