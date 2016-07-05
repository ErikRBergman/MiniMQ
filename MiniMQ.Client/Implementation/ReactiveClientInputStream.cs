namespace MiniMQ.Client.Implementation
{
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Client.Conversion;
    using MiniMQ.Client.Model;

    internal class ReactiveClientInputStream : ClientInputStream
    {
        /// <summary>
        /// The cancellation token.
        /// </summary>
        private readonly CancellationToken cancellationToken;

        /// <summary>
        /// The reactive client connection.
        /// </summary>
        private readonly IReactiveClientConnection reactiveClientConnection;

        /// <summary>
        /// The reactive connection.
        /// </summary>
        private readonly IReactiveConnection reactiveConnection;

        /// <summary>
        /// The buffer.
        /// </summary>
        private Buffer buffer = new Buffer();

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
            ClientWebSocket clientWebSocket, 
            IReactiveConnection reactiveConnection, 
            IReactiveClientConnection reactiveClientConnection, 
            CancellationToken cancellationToken) : base(clientWebSocket)
        {
            this.reactiveConnection = reactiveConnection;
            this.reactiveClientConnection = reactiveClientConnection;
            this.cancellationToken = cancellationToken;

            this.bufferReader = new BufferReader(this.buffer);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (this.isEndOfMessage)
            {
                return Task.FromResult(0);
            }

            if (!this.isFirstMessage)
            {
                return base.ReadAsync(buffer, offset, count, cancellationToken);
            }

            var result = this.bufferReader.Read(buffer, offset, count);

            if (result.IsLast)
            {
                this.isFirstMessage = false;
            }

            return Task.FromResult(result.BytesRead);
        }

        public void StartReceiving()
        {
            Task.Run(this.ReceiveNewMessage, this.cancellationToken);
        }

        private async Task ReceiveNewMessage()
        {
            var result = await this.ClientWebSocket.ReceiveAsync(this.buffer.AsArraySegment(), this.cancellationToken);

            this.buffer.Length = result.Count;

            if (result.EndOfMessage || result.CloseStatus != null)
            {
                this.isEndOfMessage = true;
                this.reactiveClientConnection.MessageReceiveDone(this);
            }

            await this.reactiveConnection.OnMessageReceived(this);

            if (result.CloseStatus != null)
            {
                this.reactiveConnection.OnConnectionClosed(CloseStatusConverison.ConvertToCloseStatus(result.CloseStatus.Value), result.CloseStatusDescription);
            }
        }
    }
}