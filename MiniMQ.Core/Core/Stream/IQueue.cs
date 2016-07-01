namespace MiniMQ.Core.Core.Stream
{
    public interface IQueue<T>
    {
        void Enqueue(T item);

        T Dequeue();
        int Count { get; }
        void Clear();
    }
}
