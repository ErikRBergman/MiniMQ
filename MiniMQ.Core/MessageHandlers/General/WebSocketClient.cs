namespace MiniMQ.Core.MessageHandlers.General
{
    using System;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Core.Core;
    using MiniMQ.Core.Core.Stream;
    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    public class WebSocketClient : IWebSocketClient, IHealthCheck
    {
        private readonly WebSocket webSocket;

        private readonly IMessageFactory messageFactory;

        private readonly TaskCompletionSource<bool> waitDisconnectTaskCompletionSource = new TaskCompletionSource<bool>();

        public WebSocketClient(WebSocket webSocket, IMessageFactory messageFactory)
        {
            this.webSocket = webSocket;
            this.messageFactory = messageFactory;
        }

        public bool IsConnected => this.webSocket.State == WebSocketState.Open;

        public async Task<bool> SendMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            if (!this.IsConnected)
                return false;

            var stream = await message.GetStream();

            if (stream == null)
                return true;

            using (stream)
            {
                await stream.CopyToAsync(this.webSocket, cancellationToken);
            }

            return true;
        }

        public async Task<bool> WaitDisconnectAsync(int milliseconds, CancellationToken cancellationToken)
        {
            var source = cancellationToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, new CancellationToken()) : new CancellationTokenSource();

            var result = await Task.WhenAny(Task.Delay(milliseconds, source.Token), this.waitDisconnectTaskCompletionSource.Task);

            if (result == this.waitDisconnectTaskCompletionSource.Task)
            {
                source.Cancel();
                return true;
            }

            return false;
        }

        public async Task<IMessage> ReceiveMessageAsync(IMessagePipeline messagePipeline, string messageUniqueIdentifier, CancellationToken token)
        {
            var inputStream = new WebSocketInputStream(this.webSocket);
            var message = await this.messageFactory.CreateMessage(inputStream, messageUniqueIdentifier);

            if (messagePipeline != null)
            {
                await messagePipeline.SendMessageAsync(message);
            }

            return message;

        }

        private void SetDisconnectWaitComplete()
        {
            this.waitDisconnectTaskCompletionSource.TrySetResult(true);            
        }

        public bool DoHealthCheck()
        {
            if (this.IsConnected == false)
            {
                this.SetDisconnectWaitComplete();
                return false;
            }

            return true;
        }
    }
}
