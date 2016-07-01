namespace MiniMQ.Core.Core.Stream
{
    using System.Collections.Generic;

    class QueueBridge<T> : IQueue<T>
    {
        private readonly Queue<T> queue;

        public QueueBridge(Queue<T> queue)
        {
            this.queue = queue;
        }

        public void Enqueue(T item)
        {
            this.queue.Enqueue(item);
        }

        public T Dequeue()
        {
            return this.queue.Dequeue();
        }

        public int Count => this.queue.Count;
        public void Clear()
        {
            this.queue.Clear();
        }
    }
}
