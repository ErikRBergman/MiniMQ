using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiniMQ.Core.MessageHandler
{
    using System.Threading.Tasks;

    using MiniMQ.Core.Message;
    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    public class MessageHandlerProducer : IMessageHandlerProducer
    {
        public MessageHandlerProducer(IMessageHandlerFactory queueMessageHandlerFactory, IMessageHandlerFactory applicationMessageHandlerFactory, IMessageHandlerFactory busMessageHandlerFactory)
        {
            this.QueueFactory = queueMessageHandlerFactory;
            this.ApplicationFactory = applicationMessageHandlerFactory;
            this.BusFactory = busMessageHandlerFactory;
        }

        public IMessageHandlerFactory QueueFactory { get; }

        public IMessageHandlerFactory BusFactory { get; }

        public IMessageHandlerFactory ApplicationFactory { get; }
    }
}