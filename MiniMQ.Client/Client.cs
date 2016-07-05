using System;
using System.Threading.Tasks;

namespace MiniMQ.Client
{
    using System.Collections.Generic;
    using System.Net.WebSockets;
    using System.Threading;

    using MiniMQ.Client.Implementation;
    using MiniMQ.Client.Model;

    public class Client : IClient
    {
        public async Task ConnectToServer(Uri serverUri, IReactiveConnection reactiveConnection, CancellationToken cancellationToken)
        {
            var client = new ClientWebSocket();
            await client.ConnectAsync(serverUri, cancellationToken);
            var connection = new ReactiveClientConnection(client, reactiveConnection, cancellationToken);


        }

        public Task<IClientConnection> ConnectToServer(Uri serverUri, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Message> ReceiveMessageOrNullAsync(Uri serverUri, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Message> ReceiveMessageAsync(Uri serverUri, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<Message>> ReceiveAllMessagesAsync(Uri serverUri, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SendMessageAsync(Uri serverUri, Message message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
