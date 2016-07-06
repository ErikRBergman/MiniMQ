namespace MiniMQ.Core.MessageHandlers.General
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Model.Core.MessageHandler;

    internal class WebSocketClientQueueSubscriber
    {
        private readonly IMessageHandler messageHandler;

        private readonly IWebSocketClient webSocketClient;

        public WebSocketClientQueueSubscriber(IMessageHandler messageHandler, IWebSocketClient webSocketClient)
        {
            this.messageHandler = messageHandler;
            this.webSocketClient = webSocketClient;
        }

        public void Subscribe()
        {
            Task.Run(this.SubscriberMethod);
        }

        private async Task SubscriberMethod()
        {
            await Task.Yield();

            var cancellationTokenSource = new CancellationTokenSource();

            var pipeline = new WebSocketDestinationMessagePipeline(this.messageHandler, this.webSocketClient);

            var receiveMessageTask = this.messageHandler.ReceiveMessageAsync(pipeline, cancellationTokenSource.Token);

            while (this.webSocketClient.IsConnected)
            {
                var pollTask = Task.Delay(TimeSpan.FromSeconds(10));

                var result = await Task.WhenAny(receiveMessageTask, pollTask);

                if (result == receiveMessageTask)
                {
                    receiveMessageTask = this.messageHandler.ReceiveMessageAsync(pipeline, cancellationTokenSource.Token);
                }
            }

            cancellationTokenSource.Cancel();
        }
    }
}
