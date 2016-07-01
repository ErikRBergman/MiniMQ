namespace MiniMQ.Core.Core.Stream
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    internal class ByteArrayBuilder : IDisposable
    {
        private const int ArraySize = 2 * 1024;

        private byte[] currentArray;

        private int currentArrayPosition = 0;

        private readonly List<byte[]> arrays = new List<byte[]>(2000);

        // private readonly Queue<byte[]> freeArrays = new Queue<byte[]>(2000);

        private IQueue<byte[]> freeArrays;

        private int currentSize = 0;

        private const int globalQueueSize = 500;

        private static readonly ConcurrentQueueMaxSizeBridge<byte[]> globalByteArrayQueue = new ConcurrentQueueMaxSizeBridge<byte[]>(new ConcurrentQueue<byte[]>(), globalQueueSize);

        public ByteArrayBuilder(bool sharedArrayQueue = false)
        {
            if (sharedArrayQueue && globalByteArrayQueue.Count < globalQueueSize)
            {
                while (globalByteArrayQueue.Count < globalQueueSize)
                {
                    globalByteArrayQueue.Enqueue(new byte[ArraySize]);
                }
            }
            
            if (sharedArrayQueue)
            {
                this.freeArrays = globalByteArrayQueue;
            }
            else
            {
                this.freeArrays = new QueueBridge<byte[]>(new Queue<byte[]>(100));
            }
            
            this.Grow();
        }

        public void Reset()
        {
            this.currentSize = 0;
            this.currentArrayPosition = 0;
            this.freeArrays.AddRange(this.arrays);
            this.arrays.Clear();
        }

        public int Length => ((this.arrays.Count - 1) * ArraySize) + this.currentArrayPosition;

        public void Add(byte b)
        {
            if (this.currentArrayPosition == this.currentArray.Length)
            {
                this.Grow();
            }

            this.AddInternal(b);

            this.currentSize++;
        }

        internal void AddInternal(byte b)
        {
            this.currentArray[this.currentArrayPosition++] = b;
        }

        internal bool IsFull => this.currentArrayPosition == ArraySize;

        public void Add(byte[] bytes)
        {
            this.Add(bytes, 0, bytes.Length);
        }

        public int FreeSpace => (this.arrays.Count*ArraySize) - this.currentSize;

        internal void Add(byte[] bytes, int offset, int count)
        {
            int index = offset;
            int remainingChars = count;

            while (remainingChars > 0)
            {
                this.GrowIfFull();

                int charsToCopy = Math.Min(this.FreeSpace, remainingChars);

                Array.Copy(bytes, index, this.currentArray, this.currentArrayPosition, charsToCopy);

                this.currentArrayPosition += charsToCopy;
                index += charsToCopy;

                this.currentSize += charsToCopy;

                remainingChars -= charsToCopy;
            }
        }

        public int Read(byte[] buffer, int offset, long position, int count)
        {
            int bytesRead = 0;

            if (position > this.Length)
            {
                throw new IndexOutOfRangeException("");
            }

            var index = this.GetArrayIndexAndPositionFromPosition(position);

            if (index.Position == -1)
            {
                throw new IndexOutOfRangeException("The requested position is out of array range");
            }

            var bytesLeft = Math.Min(count, (int)(this.Length - position));
            bytesRead = bytesLeft;

            while (bytesLeft > 0)
            {
                var isLastArray = this.IsLastArrayIndex(index.ArrayIndex);
                var currentArrayLength = this.GetArrayLength(isLastArray);

                var bytesLeftInArray = currentArrayLength - index.Position;

                var bytesToRead = Math.Min(bytesLeft, bytesLeftInArray);

                var array = this.arrays[index.ArrayIndex];
                Array.Copy(array, index.Position, buffer, offset, bytesToRead);
                offset += bytesToRead;

                index.Position = 0;
                index.ArrayIndex++;
                bytesLeft -= bytesToRead;
            }

            return bytesRead;
        }

        internal int GetArrayLength(bool isLastArray)
        {
            var currentArrayLength = isLastArray ? this.currentSize : ArraySize;
            return currentArrayLength;
        }

        internal bool IsLastArrayIndex(int arrayIndex)
        {
            return arrayIndex == this.arrays.Count - 1;
        }

        internal ArrayIndexAndPosition GetArrayIndexAndPositionFromPosition(long position)
        {
            var result = new ArrayIndexAndPosition
            {
                ArrayIndex = (int)(position / ArraySize),
                Position = (int)(position % ArraySize)
            };

            if (result.ArrayIndex >= this.arrays.Count || result.Position >= this.currentSize)
            {
                result.ArrayIndex = result.Position = -1;
            }

            return result;
        }

        internal struct ArrayIndexAndPosition
        {
            public int ArrayIndex { get; set; }
            public int Position { get; set; }
        }

        internal void GrowIfFull()
        {
            if (this.IsFull)
            {
                this.Grow();
            }
        }

        internal void Grow()
        {
            this.currentArray = this.GetNewArray();
            this.arrays.Add(this.currentArray);
            this.currentArrayPosition = 0;
        }

        internal byte[] GetNewArray()
        {
            var array = this.freeArrays.Dequeue();
            return array ?? new byte[ArraySize];
        }

        public void Dispose()
        {
            this.currentArray = null;

            if (this.freeArrays != globalByteArrayQueue)
            {
                this.freeArrays.Clear();
            }

            this.arrays.Clear();
        }
    }


    public static class QueueExtensions
    {
        public static void AddRange<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
            }
        }

        public static void AddRange<T>(this IQueue<T> queue, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
            }
        }
    }

}
