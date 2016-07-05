namespace MiniMQ.Core.MessageHandlers.InMemory.Application
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    public class MessageApplication : IMessageHandler
    {
        private readonly IMessageFactory messageFactory;

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);

        private readonly ConcurrentQueue<IMessage> requestMessages = new ConcurrentQueue<IMessage>();

        private struct RequestWaiter
        {
            public RequestWaiter(SemaphoreSlim waiter, IMessage message)
            {
                this.Waiter = waiter;
                this.Message = message;
            }

            public SemaphoreSlim Waiter;

            public IMessage Message;
        }

        private readonly ConcurrentDictionary<string, RequestWaiter> requestWaiters = new ConcurrentDictionary<string, RequestWaiter>();

        public MessageApplication(IMessageFactory messageFactory, string name)
        {
            this.messageFactory = messageFactory;
            this.Name = name;
        }

        public string Name { get; }

        public bool SupportsSendAndReceiveMessage => true;

        public bool SupportsWebSocketConnections => true;

        public IMessageFactory GetMessageFactory()
        {
            return this.messageFactory;
        }

        public async Task ReceiveMessageAsync(IMessagePipeline pipeline, CancellationToken cancellationToken)
        {
            await this.semaphore.WaitAsync(cancellationToken);
            IMessage message;

            // this will only fail if there is a bug in the program
            this.requestMessages.TryDequeue(out message);

            await pipeline.SendMessage(message);
        }

        public IMessage ReceiveMessageOrNull()
        {
            throw new NotImplementedException();
        }

        public async Task SendAndReceiveMessageAsync(IMessage message, IMessagePipeline pipeline, CancellationToken cancellationToken)
        {
            //var source = GetPooledSemaphoreSlim(0);

            var source = new SemaphoreSlim(0);

            var uniqueId = message.UniqueIdentifier;

            this.requestWaiters.TryAdd(uniqueId, new RequestWaiter(source, null));
            this.requestMessages.Enqueue(message);

            this.semaphore.Release();

            RequestWaiter waiter;

            try
            {
                await source.WaitAsync(cancellationToken);
            }
            catch (Exception)
            {
                this.requestWaiters.TryRemove(uniqueId, out waiter);
                //this.semaphorePool.ReturnObject(waiter.Waiter);
                throw;
            }

            if (this.requestWaiters.TryRemove(uniqueId, out waiter))
            {
                //this.semaphorePool.ReturnObject(waiter.Waiter);
                await pipeline.SendMessage(waiter.Message);
            }

        }

        public Task SendMessage(IMessage message)
        {
            var uniqueId = message.UniqueIdentifier;

            RequestWaiter waiter;

            if (this.requestWaiters.TryGetValue(uniqueId, out waiter))
            {
                waiter.Message = message;
                // Since waiter is a struct, we need to replace it in the dictionary
                this.requestWaiters[uniqueId] = waiter;

                waiter.Waiter.Release();

                return Task.CompletedTask;
            }

            this.requestMessages.Enqueue(message);
            this.semaphore.Release();

            return Task.CompletedTask;
        }
    }
}