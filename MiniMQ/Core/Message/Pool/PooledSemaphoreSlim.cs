namespace MiniMQ.Core.Message.Pool
{
    using System.Threading;
    using System.Threading.Tasks;

    public class PooledSemaphoreSlim : IPooledObject
    {
        private readonly SemaphoreSlim semaphoreSlim;

        private volatile int count;

        public PooledSemaphoreSlim(int initialCount)
        {
            this.semaphoreSlim = new SemaphoreSlim(initialCount);
            this.count = initialCount;
        }

        public void Release(int releaseCount = 1)
        {
            this.semaphoreSlim.Release(releaseCount);
            Interlocked.Add(ref this.count, releaseCount);
        }

        public async Task WaitAsync(CancellationToken cancellationToken)
        {
            await this.semaphoreSlim.WaitAsync(cancellationToken);
            Interlocked.Decrement(ref this.count);
        }

        private void Wait(int waitCount)
        {
            while (waitCount > 0 && this.count > 0)
            {
                this.semaphoreSlim.Wait();
                Interlocked.Decrement(ref this.count);
                waitCount--;
            }
        }



        public int Count
        {
            get
            {
                return this.count;
            }
            set
            {
                this.SetCount(value);
            }
        }

        // This should only be called when no one is using the semaphore, or it may spin up and down for a while until it sets
        private void SetCount(int value)
        {
            while (this.count != value)
            {
                var diff = value - this.count;

                if (diff > 0)
                {
                    this.Release(diff);
                }
                else if (diff < 0)
                {
                    this.Wait(-diff);
                }
                else
                {
                    return;
                }
            }
        }

        public void RealDispose()
        {

        }
    }
}