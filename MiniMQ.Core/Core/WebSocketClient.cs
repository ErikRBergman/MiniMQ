using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Core.Core
{
    using System.Net.WebSockets;
    using System.Threading;

    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;
    using MiniMQ.Core.Core.Stream;
    public class WebSocketClient : IWebSocketClient
    {
        private readonly WebSocket webSocket;

        public WebSocketClient(WebSocket webSocket)
        {
            this.webSocket = webSocket;
        }

        public bool IsConnected => this.webSocket.State == WebSocketState.Open;

        public async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            if (this.IsConnected)
            {
                var stream = await message.GetStream();
                await stream.CopyToAsync(this.webSocket, cancellationToken);
            }
        }
    }
}
