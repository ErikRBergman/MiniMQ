namespace MiniMQ.Client.Implementation
{
    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;

    internal class ClientOutputStream : Stream
    {
        private readonly WebSocket webSocket;

        private bool isEndOfMessageSent = false;

        internal ClientOutputStream(WebSocket webSocket)
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.WriteAsync(buffer, offset, count, CancellationToken.None).Wait();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (ReferenceEquals(buffer, EndOfMessage.Value))
            {
                return this.SendCloseMessageAsync();
            }

            return this.webSocket.SendAsync(new ArraySegment<byte>(buffer, offset, count), WebSocketMessageType.Binary, false, cancellationToken);
        }

        public Task SendCloseMessageAsync()
        {
            return this.webSocket.SendAsync(new ArraySegment<byte>(), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.isEndOfMessageSent == false)
            {
                // Ensure end of message is sent..                
                Console.WriteLine("Rescuing the output stream! End of message was not sent purposely by the user! Ensure you are calling WriteAsync(null, 0, 0) before closing the output stream.");

                this.SendCloseMessageAsync().Wait();
                this.isEndOfMessageSent = true;
            }

            base.Dispose(disposing);
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length { get; } = -1;

        public override long Position { get; set; } = -1;
    }
}