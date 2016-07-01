namespace MiniMQ.Core.Message.Pool
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class PooledStream : Stream, IPooledObject
    {
        private readonly ObjectPool<PooledStream> objectPool;

        private readonly Stream internalStream;

        private readonly MemoryStream internalMemoryStream;

        public PooledStream(Stream internalStream, ObjectPool<PooledStream> objectPool)
        {
            this.objectPool = objectPool;
            this.internalStream = internalStream;
            this.internalMemoryStream = internalStream as MemoryStream;
        }

        public int Capacity
        {
            get
            {
                if (this.internalMemoryStream != null)
                {
                    return this.internalMemoryStream.Capacity;
                }

                throw new NotImplementedException("The internal stream does not support capacity");
            }
            set
            {
                if (this.internalMemoryStream != null)
                {
                    this.internalMemoryStream.Capacity = value;
                }
                else
                {
                    throw new NotImplementedException("The internal stream does not support capacity");
                }

            }
        }
        
        protected override void Dispose(bool disposing)
        {
            this.objectPool.ReturnObject(this);
        }

        public override void Flush()
        {
            this.internalStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.internalStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.internalStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.internalStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.internalStream.Write(buffer, offset, count);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return this.internalStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override void Close()
        {
            // this.internalStream.Close();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.internalStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.internalStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            this.internalStream.WriteByte(value);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.internalStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.internalStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.internalStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.internalStream.EndWrite(asyncResult);
        }

        public override int ReadByte()
        {
            return this.internalStream.ReadByte();
        }

        public override int ReadTimeout
        {
            get
            {
                return this.internalStream.ReadTimeout;
            }
            set
            {
                this.internalStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return this.internalStream.WriteTimeout;
            }
            set
            {
                this.internalStream.WriteTimeout = value;
            }
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return this.internalStream.FlushAsync(cancellationToken);
        }

        public override bool CanTimeout => this.internalStream.CanTimeout;

        public override bool CanRead => this.internalStream.CanRead;

        public override bool CanSeek => this.internalStream.CanSeek;

        public override bool CanWrite => this.internalStream.CanWrite;

        public override long Length => this.internalStream.Length;

        public override long Position
        {
            get
            {
                return this.internalStream.Position;
            }
            set
            {
                this.internalStream.Position = value;
            }
        }

        public void RealDispose()
        {
            this.internalStream.Dispose();
        }
    }
}