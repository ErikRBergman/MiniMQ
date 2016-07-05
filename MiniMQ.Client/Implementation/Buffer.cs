namespace MiniMQ.Client.Implementation
{
    using System;

    internal struct Buffer
    {
        public Buffer(byte[] byteBuffer, int length)
        {
            this.Contents = byteBuffer;
            this.Length = length;
        }

        public static Buffer CreateDefault()
        {
             return new Buffer(new byte[1024], 0);
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