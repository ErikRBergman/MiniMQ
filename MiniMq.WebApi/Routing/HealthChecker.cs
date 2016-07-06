using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniMq.WebApi.Routing
{
    using System.Collections.Concurrent;
    using System.Threading;

    using MiniMQ.Core.Core;

    public class HealthChecker : IHealthChecker
    {
        private readonly TimeSpan checkInterval;

        private int isStarted = 0;

        private TaskCompletionSource<bool> startTaskCompletionSource = new TaskCompletionSource<bool>();

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private ConcurrentDictionary<IHealthCheckBase, bool> targets = new ConcurrentDictionary<IHealthCheckBase, bool>();

        public HealthChecker(TimeSpan checkInterval)
        {
            this.checkInterval = checkInterval;
        }

        public void Add(IHealthCheckBase healthCheck)
        {
            this.targets.TryAdd(healthCheck, false);
        }

        public Task Start()
        {
            if (Interlocked.CompareExchange(ref this.isStarted, 1, 0) == 0)
            {
                this.StartInternal();
            }

            return this.startTaskCompletionSource.Task;
        }

        private void StartInternal()
        {
            Task.Run(this.CheckWorkerAsync);
        }

        private async Task CheckWorkerAsync()
        {
            do
            {
                await Task.Delay(
                    this.checkInterval, 
                    this.cancellationTokenSource.Token);
                await this.CheckTargets();
            }
            while (true);
        }

        private async Task CheckTargets()
        {
            var itemsToRemove = new List<IHealthCheckBase>(10);

            foreach (var target in this.targets)
            {
                var result = false;

                var healthCheckBase = target.Key;

                var asyncTarget = healthCheckBase as IHealthCheckAsync;

                if (asyncTarget != null)
                {
                    result = await asyncTarget.DoHealthCheckAsync();
                }
                else
                {
                    var syncTarget = healthCheckBase as IHealthCheck;

                    if (syncTarget != null)
                    {
                        result = syncTarget.DoHealthCheck();
                    }
                }

                if (result == false)
                {
                    itemsToRemove.Add(healthCheckBase);
                }
            }

            foreach (var item in itemsToRemove)
            {
                bool dummy;
                this.targets.TryRemove(item, out dummy);
            }
        }
    }
}
