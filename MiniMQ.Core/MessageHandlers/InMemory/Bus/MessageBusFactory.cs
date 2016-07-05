namespace MiniMQ.Core.MessageHandlers.InMemory.Bus
{
    using System;
    using System.Threading.Tasks;

    using MiniMQ.Model.Core.MessageHandler;

    public class MessageBusFactory : IMessageHandlerFactory
    {
        public Task<IMessageHandler> Create(string name)
        {
            throw new NotImplementedException();
        }
    }
}
