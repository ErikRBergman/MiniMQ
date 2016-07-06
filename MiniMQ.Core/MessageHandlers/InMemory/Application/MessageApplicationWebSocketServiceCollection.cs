using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Core.MessageHandlers.InMemory.Application
{
    using System.Collections.Concurrent;

    using MiniMQ.Core.MessageHandlers.InMemory.Queue;
    using MiniMQ.Model.Core.MessageHandler;

    internal class MessageApplicationWebSocketServiceCollection
    {
        private ConcurrentQueue<IWebSocketSubscriber> availableServices = new ConcurrentQueue<IWebSocketSubscriber>();

        private ConcurrentQueue<IWebSocketSubscriber> busyServices = new ConcurrentQueue<IWebSocketSubscriber>();

        public IWebSocketSubscriber

    }
}
