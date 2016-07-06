using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Model.Core
{
    using System.Collections;
    using System.Collections.Concurrent;

    public class ConcurrentList<T>: IReadOnlyCollection<T>
    {
        private object lockObject = new object();

        private List<T> list;

        public ConcurrentList(int capacity)
        {
            this.list = new List<T>(capacity);
        }

        public ConcurrentList(int capacity, IReadOnlyCollection<T> collection)
        {
            this.list = new List<T>(Math.Max(collection.Count, capacity));
            this.list.AddRange(collection);
        }

        public ConcurrentList(int capacity, ICollection<T> collection)
        {
            this.list = new List<T>(Math.Max(collection.Count, capacity));
            this.list.AddRange(collection);
        }

        public ConcurrentList(int capacity, IEnumerable<T> collection)
        {
            this.list = new List<T>(capacity);
            this.list.AddRange(collection);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.GetListCopy().GetEnumerator();
        }

        private List<T> GetListCopy()
        {
            lock (this.lockObject)
            {
                return new List<T>(this.list);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                lock (this.lockObject)
                {

                    ConcurrentQueue<>
                    return this.list.Count;
                }
            }
        }
    }
}
