namespace MiniMQ.Core.MessageHandlers.General
{
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

        private readonly TaskCompletionSource<bool> waitDisconnectTaskCompletionSource = new TaskCompletionSource<bool>();

        public WebSocketClient(WebSocket webSocket)
        {
            this.webSocket = webSocket;
        }

        public bool IsConnected => this.webSocket.State == WebSocketState.Open;

        public async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            if (this.IsConnected)
            {
                var stream = await message.GetStream();
                await stream.CopyToAsync(this.webSocket, cancellationToken);
            }
        }

        public Task WaitDisconnectAsync() => this.waitDisconnectTaskCompletionSource.Task;

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
