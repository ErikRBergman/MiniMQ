namespace MiniMQ.Core.Core.Stream
{
    using System.Collections.Concurrent;

    class ConcurrentQueueMaxSizeBridge<T> : IQueue<T>
    {
        private ConcurrentQueue<T> _queue;
        private readonly int _maxSize;

        public ConcurrentQueueMaxSizeBridge(ConcurrentQueue<T> queue, int maxSize = -1)
        {
            this._queue = queue;
            this._maxSize = maxSize;
        }

        public void Enqueue(T item)
        {
            if (this._maxSize == -1 || this._maxSize > this.Count)
            {
                this._queue.Enqueue(item);
            }
        }

        public T Dequeue()
        {
            T item;

            if (this._queue.TryDequeue(out item))
            {
                return item;
            }

            return default(T);
        }

        public int Count => this._queue.Count;
        public void Clear()
        {
            this._queue = new ConcurrentQueue<T>();
        }
    }
}
