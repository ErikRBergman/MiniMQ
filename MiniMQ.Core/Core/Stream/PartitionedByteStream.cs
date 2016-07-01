namespace MiniMQ.Core.Core.Stream
{

    /// <summary>
    /// A partitioned byte stream. Borrowed from Erik Bergmans private projects...
    /// </summary>
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class PartitionedByteStream : Stream
    {
        private readonly ByteArrayBuilder byteArrayBuilder;

        public PartitionedByteStream(bool sharedGlobalBuffer)
        {
            this.byteArrayBuilder = new ByteArrayBuilder(sharedGlobalBuffer);
        }

        public override void Flush()
        {

        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var newPosition = this.Position;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPosition = offset;
                    break;
                case SeekOrigin.Current:
                    newPosition += offset;
                    break;
                case SeekOrigin.End:
                    newPosition -= offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            if (newPosition < 0 || newPosition > this.Length)
            {
                throw new IndexOutOfRangeException("Seek would put position out of range");
            }

            return this.Position = newPosition;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.Position == this.Length)
            {
                return 0;
            }

            var result = this.byteArrayBuilder.Read(buffer, offset, this.Position, count);
            this.Position += result;
            return result;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.Read(buffer, offset, count));
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            this.Write(buffer, offset, count);
            return Task.CompletedTask;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.byteArrayBuilder.Add(buffer, offset, count);
            this.Position += count;
        }

        public void Write(byte b)
        {
            this.byteArrayBuilder.Add(b);
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => this.byteArrayBuilder.Length;

        public override long Position { get; set; }
    }
}

