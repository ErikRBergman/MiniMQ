using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiniMQ.Core.MessageHandler
{
    using System.Threading.Tasks;

    using MiniMQ.Core.Message;
    using MiniMQ.MessageHandlers.Application;
    using MiniMQ.MessageHandlers.Queue;

    public class MessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly IMessageFactory messageFactory;

        private readonly IMessageFactory applicationMessageFactory;

        public MessageHandlerFactory(IMessageFactory messageFactory, IMessageFactory applicationMessageFactory)
        {
            this.messageFactory = messageFactory;
            this.applicationMessageFactory = applicationMessageFactory;
        }

        public Task<IMessageHandler> CreateQueue(string queueName)
        {
            return Task.FromResult((IMessageHandler)new MessageQueue(this.messageFactory, queueName));
        }

        public Task<IMessageHandler> CreateBus(string busName)
        {
            throw new NotImplementedException();
        }

        public Task<IMessageHandler> CreateApplication(string applicationName)
        {
            return Task.FromResult((IMessageHandler)new MessageApplication(this.applicationMessageFactory, applicationName));
        }
    }
}