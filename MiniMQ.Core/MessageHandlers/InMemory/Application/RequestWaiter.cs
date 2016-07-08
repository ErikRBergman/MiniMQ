using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Core.MessageHandlers.InMemory.Application
{
    using System.Threading;

    using MiniMQ.Core.Core;
    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    internal struct RequestWaiter
    {
        private readonly SemaphoreSlim waiter;

        public RequestWaiter(IMessagePipeline pipeline)
        {
            this.waiter = new SemaphoreSlim(0);
            this.Pipeline = pipeline;
        }

        public IMessagePipeline Pipeline { get; }

        public void Release()
        {
            this.waiter.Release();
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            return this.waiter.WaitAsync(cancellationToken);
        }

        public async Task<bool> SendAndReleaseAsync(IMessage message, bool releaseEvenIfSendMessageFails = true)
        {
            if (this.Pipeline != null)
            {
                var result = await this.Pipeline.SendMessageAsync(message);

                if (releaseEvenIfSendMessageFails || result)
                {
                    this.Release();
                }

                return result;
            }

            return false;
        }

    }
}
