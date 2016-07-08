namespace MiniMQ.Core.MessageHandlers.InMemory.Queue
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Core.MessageHandlers.General;
    using MiniMQ.Model.Core.MessageHandler;

    internal class ApplicationWebSocketSubscriber : IWebSocketSubscriber
    {
        private readonly IMessageHandler messageHandler;

        private readonly IWebSocketClient webSocketClient;

        private readonly IMessagePipeline pipeline;

        private readonly WebSubscriberSettings settings;

        private readonly CancellationTokenSource cancellationTokenSource;

        public ApplicationWebSocketSubscriber(
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

            var disconnectWaitCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.cancellationTokenSource.Token, new CancellationToken());

            var disconnectionTask = this.webSocketClient.WaitDisconnectAsync(-1, disconnectWaitCancellationTokenSource.Token);
            var receiveHandlerMessageTask = this.messageHandler.ReceiveMessageAsync(this.pipeline, disconnectWaitCancellationTokenSource.Token);

            var completedTask = await Task.WhenAny(receiveHandlerMessageTask, disconnectionTask);

            // Snatch a copy, since Cancel may complete it
            var disconnectionTaskCompleted = disconnectionTask.IsCompleted;

            // Cancel the other one regardless which one completed
            disconnectWaitCancellationTokenSource.Cancel();

            if (disconnectionTaskCompleted)
            {
                return;
            }



        }
    }
}
