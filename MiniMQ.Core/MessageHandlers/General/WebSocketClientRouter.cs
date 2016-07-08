using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Core.MessageHandlers.General
{
    using System.Net.WebSockets;
    using System.Threading;

    using MiniMQ.Model.Core.MessageHandler;

    public class WebSocketClientRouter
    {
        private readonly WebSocket webSocket;

        private readonly IMessageHandler messageHandler;

        public WebSocketClientRouter(WebSocket webSocket, IMessageHandler messageHandler)
        {
            this.webSocket = webSocket;
            this.messageHandler = messageHandler;
        }

        public async Task ServeClientAsync()
        {
            var inputBuffer = new byte[1024];

            while (this.webSocket.State == WebSocketState.Open)
            {
                var result = await this.webSocket.ReceiveAsync(new ArraySegment<byte>(inputBuffer), CancellationToken.None);



                await this.webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("210 OK, fred")), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

    }
}
