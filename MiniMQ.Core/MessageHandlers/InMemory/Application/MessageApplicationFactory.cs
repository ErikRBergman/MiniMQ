namespace MiniMQ.Core.MessageHandlers.InMemory.Application
{
    using System.Threading.Tasks;

    using MiniMQ.Core.MessageHandlers.General;

    using Model.Core.Message;
    using Model.Core.MessageHandler;

    public class MessageApplicationFactory : IMessageHandlerFactory
    {
        private readonly IMessageFactory applicationMessageHandlerFactory;

        private readonly IWebSocketSubscriberFactory webSocketSubscriberFactory;

        public MessageApplicationFactory(
            IMessageFactory applicationMessageHandlerFactory, 
            IWebSocketSubscriberFactory webSocketSubscriberFactory)
        {
            this.applicationMessageHandlerFactory = applicationMessageHandlerFactory;
            this.webSocketSubscriberFactory = webSocketSubscriberFactory;
        }

        public Task<IMessageHandler> Create(string applicationName)
        {
            return Task.FromResult(
                (IMessageHandler)new MessageApplication(
                    applicationName, 
                    this.applicationMessageHandlerFactory, 
                    this.webSocketSubscriberFactory));
        }
    }
}
