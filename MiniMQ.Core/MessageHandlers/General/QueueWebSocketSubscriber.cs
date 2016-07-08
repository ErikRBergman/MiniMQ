namespace MiniMQ.Core.MessageHandlers.InMemory.Queue
{
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Core.MessageHandlers.General;
    using MiniMQ.Model.Core.MessageHandler;

    internal class QueueWebSocketSubscriber : IWebSocketSubscriber
    {
        private readonly IMessageHandler messageHandler;

        private readonly IWebSocketClient webSocketClient;

        private readonly IMessagePipeline pipeline;

        private readonly WebSubscriberSettings settings;

        private readonly CancellationTokenSource cancellationTokenSource;

        public QueueWebSocketSubscriber(
            IMessageHandler messageHandler, 
            IWebSocketClient webSocketClient, 
            IMessagePipeline pipeline, 
            WebSubscriberSettings settings)
        {
            this.messageHandler = messageHandler;
            this.webSocketClient = webSocketClient;
            this.pipeline = pipeline;
            this.settings = settings;
            this.cancellationTokenSource = new CancellationTokenSource();

        }

        public void Cancel()
        {
            this.cancellationTokenSource.Cancel();
        }

        public void Subscribe()
        {
            Task.Run(this.WorkerMethodAsync);
        }

        private async Task WorkerMethodAsync()
        {
            Task receiveHandlerMessageTask = null;

            while (this.webSocketClient.IsConnected && this.cancellationTokenSource.IsCancellationRequested == false)
            {
                if (receiveHandlerMessageTask == null)
                {
                    receiveHandlerMessageTask = this.messageHandler.ReceiveMessageAsync(this.pipeline, this.cancellationTokenSource.Token);
                }

                var pollTask = Task.Delay(this.settings.ConnectionStatusCheckInterval, this.cancellationTokenSource.Token);

                var result = await Task.WhenAny(receiveHandlerMessageTask, pollTask);

                if (result == receiveHandlerMessageTask)
                {
                    receiveHandlerMessageTask = null;
                }
            }

            this.cancellationTokenSource.Cancel();
        }
    }
}
