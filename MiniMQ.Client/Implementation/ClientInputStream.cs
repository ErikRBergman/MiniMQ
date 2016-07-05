namespace MiniMQ.Client.Implementation
{
    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    internal class ClientInputStream : Stream
    {
        protected readonly ClientWebSocket ClientWebSocket;

        private bool isFinished = false;

        public ClientInputStream(ClientWebSocket clientWebSocket)
        {
            this.ClientWebSocket = clientWebSocket;
        }

        public override void Flush()
        {
            
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (this.isFinished)
            {
                return 0;
            }

            var result = await this.ClientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), cancellationToken);

            if (result.EndOfMessage)
            {
                this.isFinished = true;
            }

            return result.Count;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.ReadAsync(buffer, offset, count).Result;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead { get; }

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length { get; } = -1;

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}