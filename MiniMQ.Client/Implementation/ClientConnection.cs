namespace MiniMQ.Client.Implementation
{
    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    internal class ClientConnection : IClientConnection
    {
        protected readonly ClientWebSocket clientWebSocket;

        public ClientConnection(ClientWebSocket clientWebSocket)
        {
            this.clientWebSocket = clientWebSocket;
        }

        public Task CloseAsync()
        {
            return this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }

        public Task SendAsync(ArraySegment<byte> data, MessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            return this.clientWebSocket.SendAsync(data, MessageTypeConverter.ConvertToWebSocketMessageType(messageType), endOfMessage, cancellationToken);
        }

        public async Task<ReceiveResult> ReceiveAsync(ArraySegment<byte> data, CancellationToken cancellationToken)
        {
            return ResultConverter.ConvertToRecieveResult(await this.clientWebSocket.ReceiveAsync(data, cancellationToken));
        }

        public Stream GetOutputStream(bool bufferingStream = true)
        {
            return new ClientOutputStream(this.clientWebSocket);
        }

        public Stream GetInputStream(bool bufferingStream = true)
        {
            return new ClientInputStream(this.clientWebSocket);
        }
    }
}