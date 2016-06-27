using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiniMQ.Core.MessageHandler
{
    using System.Threading.Tasks;

    using MiniMQ.Application;
    using MiniMQ.Core.Message;
    using MiniMQ.MessageHandlers.Queue;

    public class MessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly IMessageFactory messageFactory;

        public MessageHandlerFactory(IMessageFactory messageFactory)
        {
            this.messageFactory = messageFactory;
        }

        public Task<IMessageHandler> CreateQueue(string queueName)
        {
            return Task.FromResult((IMessageHandler)new MessageQueue(this.messageFactory));
        }

        public Task<IMessageHandler> CreateBus(string busName)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageHandler> CreateApplication(string applicationName)
        {
            return Task.FromResult((IMessageHandler)new MessageApplication(this.messageFactory));
        }
    }
}