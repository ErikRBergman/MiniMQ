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
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);
        private readonly ConcurrentQueue<IMessage> requestMessages = new ConcurrentQueue<IMessage>();
        private readonly MessageApplicationWebSocketServiceCollection serviceCollection;
        private readonly ConcurrentDictionary<string, RequestWaiter> requestWaiters = new ConcurrentDictionary<string, RequestWaiter>();

        public MessageApplication(string name, IMessageFactory messageFactory, IWebSocketSubscriberFactory webSocketSubscriberFactory)
        {
            this.MessageFactory = messageFactory;
            this.Name = name;
            this.serviceCollection = new MessageApplicationWebSocketServiceCollection(this);
        }

        public string Name { get; }

        public bool SupportsSendAndReceiveMessage => true;

        public bool SupportsWebSocketConnections => true;

        public IMessageFactory MessageFactory { get; }

        private Task WaitForNewMessageAsync(CancellationToken cancellationToken)
        {
            return this.semaphore.WaitAsync(cancellationToken);
        }

        public async Task<IMessage> ReceiveMessageAsync(IMessagePipeline pipeline, CancellationToken cancellationToken)
        {
            await this.WaitForNewMessageAsync(cancellationToken);

            IMessage message;
            this.requestMessages.TryDequeue(out message);

            if (pipeline != null)
            {
                await pipeline.SendMessageAsync(message);
            }

            return message;
        }

        public IMessage ReceiveMessageOrNull()
        {
            throw new NotImplementedException();
        }

        public async Task<IMessage> SendAndReceiveMessageAsync(IMessage message, IMessagePipeline returnMessagePipeline, CancellationToken cancellationToken)
        {
            var uniqueId = message.UniqueIdentifier;

            var pipeline = new MessageSniffingPipeline(returnMessagePipeline);
            var waiter = new RequestWaiter(pipeline);

            this.requestWaiters.TryAdd(uniqueId, waiter);

            try
            {
                this.SubmitMessageToQueue(message);
                await waiter.WaitAsync(cancellationToken);
            }
            finally
            {
                this.RemoveWaiter(uniqueId);
            }

            return pipeline.Message;
        }

        private void SubmitMessageToQueue(IMessage message)
        {
            this.requestMessages.Enqueue(message);
            this.semaphore.Release();
        }

        private RequestWaiter RemoveWaiter(string uniqueId)
        {
            RequestWaiter waiter;
            this.requestWaiters.TryRemove(uniqueId, out waiter);
            return waiter;
        }

        public async Task SendMessageAsync(IMessage message)
        {
            var uniqueId = message.UniqueIdentifier;

            RequestWaiter waiter;
            if (this.requestWaiters.TryGetValue(uniqueId, out waiter))
            {
                await waiter.SendAndReleaseAsync(message);
            }

            // Message was sent to the application without the need for a response - fire and forget
            this.SubmitMessageToQueue(message);
        }

        public Task RegisterWebSocket(IWebSocketClient webSocketClient)
        {
            // At this point, only servers can connect with web sockets
            return this.serviceCollection.RunServiceAsync(webSocketClient);
        }
    }
}