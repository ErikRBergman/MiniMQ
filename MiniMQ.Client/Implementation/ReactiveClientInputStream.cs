namespace MiniMQ.Client.Implementation
{
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Client.Conversion;
    using MiniMQ.Client.Model;

    internal class ReactiveClientInputStream : WebSocketInputStream
    {
        /// <summary>
        /// The cancellation token.
        /// </summary>
        private readonly CancellationToken cancellationToken;

        /// <summary>
        /// The reactive client connection.
        /// </summary>
        private readonly IReactiveClientConnectionCallback reactiveClientConnection;

        /// <summary>
        /// The reactive connection.
        /// </summary>
        private readonly IReactiveConnection reactiveConnection;

        /// <summary>
        /// The buffer.
        /// </summary>
        private Buffer buffer = Buffer.CreateDefault();

        /// <summary>
        /// The buffer reader.
        /// </summary>
        private BufferReader bufferReader;

        /// <summary>
        /// The is end of message.
        /// </summary>
        private bool isEndOfMessage;

        /// <summary>
        /// The is first message.
        /// </summary>
        private bool isFirstMessage = true;

        public ReactiveClientInputStream(
            WebSocket webSocket, 
            IReactiveConnection reactiveConnection, 
            IReactiveClientConnectionCallback reactiveClientConnection, 
            CancellationToken cancellationToken) : base(webSocket)
        {
            this.reactiveConnection = reactiveConnection;
            this.reactiveClientConnection = reactiveClientConnection;
            this.cancellationToken = cancellationToken;

            this.bufferReader = new BufferReader(this.buffer);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (this.isEndOfMessage)
            {
                if (this.bufferReader.IsEndOfStream)
                {
                    return 0;
                }
            }

            if (!this.isFirstMessage)
            {
                var bytesRead = await base.ReadAsync(buffer, offset, count, cancellationToken);
                return bytesRead;
            }

            var result = this.bufferReader.Read(buffer, offset, count);

            if (result.IsLast)
            {
                this.isFirstMessage = false;
            }

            return result.BytesRead;
        }

        public void StartReceiving()
        {
            Task.Run(this.ReceiveNewMessage, this.cancellationToken);
        }

        private async Task ReceiveNewMessage()
        {
            var result = await this.webSocket.ReceiveAsync(this.buffer.AsArraySegment(), this.cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                this.reactiveConnection.OnConnectionClosed(CloseStatusConverison.ConvertToCloseStatus(result.CloseStatus.Value), result.CloseStatusDescription);
                return;
            }

            if (result.EndOfMessage)
            {
                this.isEndOfMessage = true;
            }

            this.buffer.Length = result.Count;

            await this.reactiveConnection.OnMessageReceivedAsync(this);

            if (result.EndOfMessage)
            {
                this.isEndOfMessage = true;
                this.reactiveClientConnection.MessageReceiveDone(this);
            }

            if (result.CloseStatus != null)
            {
                this.reactiveConnection.OnConnectionClosed(CloseStatusConverison.ConvertToCloseStatus(result.CloseStatus.Value), result.CloseStatusDescription);
            }
        }
    }
}