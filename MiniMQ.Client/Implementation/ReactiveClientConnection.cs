namespace MiniMQ.Client.Implementation
{
    using System.Net.WebSockets;
    using System.Threading;

    using Model;

    internal class ReactiveClientConnection : ClientConnection, IReactiveClientConnection
    {
        private readonly IReactiveConnection reactiveConnection;

        private readonly CancellationToken cancellationToken;

        public ReactiveClientConnection(ClientWebSocket clientWebSocket, IReactiveConnection reactiveConnection, CancellationToken cancellationToken) : base(clientWebSocket)
        {
            this.reactiveConnection = reactiveConnection;
            this.cancellationToken = cancellationToken;
            this.StartReceivingNewMessage();
        }

        private void StartReceivingNewMessage()
        {
            var inputStream = new ReactiveClientInputStream(this.clientWebSocket, this.reactiveConnection, this, this.cancellationToken);
            inputStream.StartReceiving();
        }

        public void MessageReceiveDone(ReactiveClientInputStream stream)
        {
            this.StartReceivingNewMessage();
        }
    }
}
