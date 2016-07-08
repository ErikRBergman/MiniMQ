namespace MiniMQ.Client.Implementation
{
    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    internal class WebSocketInputStream : Stream
    {
        protected readonly WebSocket webSocket;

        private bool isFinished = false;

        public WebSocketInputStream(WebSocket webSocket)
        {
            this.webSocket = webSocket;
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

            var result = await this.webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), cancellationToken);

            if (result.EndOfMessage)
            {
                this.isFinished = true;
            }

            return result.Count;
        }

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            var buffer = new byte[bufferSize];

            bool keepReading = true;

            while (keepReading)
            {
                var result = await this.webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, 0, bufferSize), cancellationToken);

                if (result.Count > 0)
                {
                    await destination.WriteAsync(buffer, 0, result.Count, cancellationToken);
                }

                if (result.EndOfMessage)
                {
                    keepReading = false;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.ReadAsync(buffer, offset, count).Result;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;

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