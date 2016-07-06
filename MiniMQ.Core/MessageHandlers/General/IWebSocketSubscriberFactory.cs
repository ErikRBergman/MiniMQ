using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Core.MessageHandlers.General
{
    using MiniMQ.Core.MessageHandlers.InMemory.Queue;
    using MiniMQ.Model.Core.MessageHandler;

    public interface IWebSocketSubscriberFactory
    {
        IWebSocketSubscriber CreateSubscriber(IMessageHandler messageHandler, IWebSocketClient webSocketClient);
    }
}
