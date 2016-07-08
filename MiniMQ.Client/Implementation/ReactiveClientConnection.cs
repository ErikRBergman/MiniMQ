namespace MiniMQ.Client.Implementation
{
    using System.Net.WebSockets;
    using System.Threading;

    using Model;

    internal class ReactiveClientConnection : ClientConnection, IReactiveClientConnection, IReactiveClientConnectionCallback
    {
        private readonly IReactiveConnection reactiveConnection;

        private readonly CancellationToken cancellationToken;

        private CancellationTokenSource receiveMessagesCancellationTokenSource; 

        public ReactiveClientConnection(WebSocket webSocket, IReactiveConnection reactiveConnection, CancellationToken cancellationToken) : base(webSocket)
        {
            this.reactiveConnection = reactiveConnection;
            this.cancellationToken = cancellationToken;
        }

        public void StartReceivingNewMessage()
        {
            this.receiveMessagesCancellationTokenSource = new CancellationTokenSource();

            var inputStream = new ReactiveClientInputStream(
                this.WebSocket, this.reactiveConnection, this, CancellationTokenSource.CreateLinkedTokenSource(this.cancellationToken, this.receiveMessagesCancellationTokenSource.Token).Token);
            inputStream.StartReceiving();
        }

        public void StopReceivingMessages()
        {
            this.receiveMessagesCancellationTokenSource.Cancel();
        }

        void IReactiveClientConnectionCallback.MessageReceiveDone(ReactiveClientInputStream stream)
        {
            this.StartReceivingNewMessage();
        }
    }
}
