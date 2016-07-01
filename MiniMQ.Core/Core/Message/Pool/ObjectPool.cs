namespace MiniMQ.Core.Message.Pool
{
    using System;
    using System.Collections.Concurrent;

    public interface IPooledObject
    {
        void RealDispose();
    }

    public class ObjectPool<T>
    {
        private readonly int maxSize;

        private readonly ConcurrentQueue<T> pool = new ConcurrentQueue<T>();

        public ObjectPool(int maxSize)
        {
            this.maxSize = maxSize;
        }

        public T GetNewObject(Func<T> createNewObject)
        {
            T obj;

            if (this.pool.TryDequeue(out obj))
            {
                return obj;
            }

            return createNewObject.Invoke();
        }

        public void ReturnObject(T obj)
        {
            if (this.pool.Count < this.maxSize)
            {
                this.pool.Enqueue(obj);
            }
            else
            {
                var pooledObject = obj as IPooledObject;
                pooledObject?.RealDispose();
            }
        }

    }
}