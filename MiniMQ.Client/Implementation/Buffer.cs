namespace MiniMQ.Client.Implementation
{
    using System;

    internal class Buffer
    {
        public Buffer(byte[] byteBuffer, int length)
        {
            this.Contents = byteBuffer;
            this.Length = length;
        }

        public static Buffer CreateDefault()
        {
            const int defaultSize = 1024;

             return new Buffer(new byte[defaultSize], defaultSize);
        }

        public byte[] Contents { get; private set; }

        public int Length { get; set; }

        public ArraySegment<byte> AsArraySegment()
        {
            return new ArraySegment<byte>(this.Contents, 0, this.Length);
        }
    }

    /// <summary>
    /// The buffer reader.
    /// </summary>
    internal struct BufferReader
    {
        private readonly Buffer buffer;

        private int position;

        public BufferReader(Buffer buffer, int position = 0)
        {
            this.buffer = buffer;
            this.position = position;
        }

        public bool IsEndOfStream => this.position >= this.buffer.Length;

        public struct ReadResult
        {
            public bool IsLast { get; set; }

            public int BytesRead { get; set; }

        }

        public ReadResult Read(byte[] destination, int offset, int bytesToRead)
        {
            var bytesLeft = this.buffer.Length - this.position;

            if (bytesToRead > bytesLeft)
            {
                bytesToRead = bytesLeft;
            }

            var result = new ReadResult
                         {
                             BytesRead = bytesToRead
                         };


            if (bytesToRead == bytesLeft)
            {
                result.IsLast = true;
            }

            Array.Copy(this.buffer.Contents, this.position, destination, offset, bytesToRead);

            this.position += bytesToRead;

            return result;
        }
    }
}