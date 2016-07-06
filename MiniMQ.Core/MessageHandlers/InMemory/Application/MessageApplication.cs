namespace MiniMQ.Core.MessageHandlers.InMemory.Application
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using General;
    using Model.Core.Message;
    using Model.Core.MessageHandler;

    public class MessageApplication : IMessageHandler
    {
        private readonly IMessageFactory messageFactory;

        private readonly IWebSocketSubscriberFactory webSocketSubscriberFactory;

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);

        private readonly ConcurrentQueue<IMessage> requestMessages = new ConcurrentQueue<IMessage>();

        private struct RequestWaiter
        {
            public RequestWaiter(SemaphoreSlim waiter, IMessagePipeline pipeline, IMessage message = null)
            {
                this.Waiter = waiter;
                this.Message = message;
                this.Pipeline = pipeline;
            }

            public readonly SemaphoreSlim Waiter;

            public IMessage Message;

            public IMessagePipeline Pipeline { get; }
        }

        private readonly ConcurrentDictionary<string, RequestWaiter> requestWaiters = new ConcurrentDictionary<string, RequestWaiter>();

        public MessageApplication(string name, IMessageFactory messageFactory, IWebSocketSubscriberFactory webSocketSubscriberFactory)
        {
            this.messageFactory = messageFactory;
            this.webSocketSubscriberFactory = webSocketSubscriberFactory;
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

            await pipeline.SendMessageAsync(message);
        }

        public IMessage ReceiveMessageOrNull()
        {
            throw new NotImplementedException();
        }

        public async Task SendAndReceiveMessageAsync(IMessage message, IMessagePipeline returnMessagePipeline, CancellationToken cancellationToken)
        {
            var source = new SemaphoreSlim(0);

            var uniqueId = message.UniqueIdentifier;

            this.requestWaiters.TryAdd(uniqueId, new RequestWaiter(source, returnMessagePipeline));
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
                throw;
            }

            if (this.requestWaiters.TryRemove(uniqueId, out waiter))
            {
                if (waiter.Message != null)
                {
                    await returnMessagePipeline.SendMessageAsync(waiter.Message);
                }
            }

        }

        public async Task SendMessageAsync(IMessage message)
        {
            var uniqueId = message.UniqueIdentifier;

            RequestWaiter waiter;

            if (this.requestWaiters.TryGetValue(uniqueId, out waiter))
            {
                var pipeline = waiter.Pipeline;

                if (pipeline != null)
                {
                    await pipeline.SendMessageAsync(message);
                }
                else
                {
                    waiter.Message = message;
                    // Since waiter is a struct, we need to replace it in the dictionary
                    this.requestWaiters[uniqueId] = waiter;
                }

                waiter.Waiter.Release();
            }

            // Message was sent to the application without the need for a response - fire and forget
            this.requestMessages.Enqueue(message);
            this.semaphore.Release();

        }

        public void RegisterWebSocket(IWebSocketClient webSocketClient)
        {
            // At this point, only servers can connect with web sockets

            var subscriber = this.webSocketSubscriberFactory.CreateSubscriber(this, webSocketClient);

            subscriber.Subscribe();


            throw new NotImplementedException();
        }
    }
}