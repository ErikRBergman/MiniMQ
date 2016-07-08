namespace MiniMQ.Client.Implementation
{
    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    internal class ClientConnection : IClientConnection
    {
        protected readonly WebSocket WebSocket;

        public ClientConnection(WebSocket webSocket)
        {
            this.WebSocket = webSocket;
        }

        public Task CloseAsync()
        {
            return this.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }

        public virtual Task SendAsync(ArraySegment<byte> data, MessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            return this.WebSocket.SendAsync(data, MessageTypeConverter.ConvertToWebSocketMessageType(messageType), endOfMessage, cancellationToken);
        }

        public async Task<ReceiveResult> ReceiveAsync(ArraySegment<byte> data, CancellationToken cancellationToken)
        {
            return ResultConverter.ConvertToRecieveResult(await this.WebSocket.ReceiveAsync(data, cancellationToken));
        }

        public Stream GetOutputStream(bool bufferingStream = true)
        {
            return new ClientOutputStream(this.WebSocket);
        }

        public Stream GetInputStream(bool bufferingStream = true)
        {
            return new WebSocketInputStream(this.WebSocket);
        }
    }
}