namespace MiniMQ.Core.MessageHandlers.InMemory.Application
{
    using System.Threading.Tasks;

    using Model.Core.Message;
    using Model.Core.MessageHandler;

    public class MessageApplicationFactory : IMessageHandlerFactory
    {
        private readonly IMessageFactory applicationMessageHandlerFactory;

        public MessageApplicationFactory(IMessageFactory applicationMessageHandlerFactory)
        {
            this.applicationMessageHandlerFactory = applicationMessageHandlerFactory;
        }

        public Task<IMessageHandler> Create(string applicationName)
        {
            return Task.FromResult((IMessageHandler)new MessageApplication(this.applicationMessageHandlerFactory, applicationName));
        }
    }
}
