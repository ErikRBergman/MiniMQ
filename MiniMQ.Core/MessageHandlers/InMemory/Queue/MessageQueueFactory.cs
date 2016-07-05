namespace MiniMQ.Core.MessageHandlers.InMemory.Queue
{
    using System.Threading.Tasks;

    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    public class MessageQueueFactory : IMessageHandlerFactory
    {
        private readonly IMessageFactory applicationMessageHandlerFactory;

        public MessageQueueFactory(IMessageFactory applicationMessageHandlerFactory)
        {
            this.applicationMessageHandlerFactory = applicationMessageHandlerFactory;
        }

        public Task<IMessageHandler> Create(string applicationName)
        {
            return Task.FromResult((IMessageHandler)new InMemoryMessageQueue(this.applicationMessageHandlerFactory, applicationName));
        }
    }
}
