using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Model.Core.MessageHandler
{
    using System.Threading;

    using MiniMQ.Model.Core.Message;

    public interface IWebSocketClient
    {
        bool IsConnected { get; }

        Task SendMessageAsync(IMessage message, CancellationToken cancellationToken);
    }
}
