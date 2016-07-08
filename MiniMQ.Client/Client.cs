using System;
using System.Threading.Tasks;

namespace MiniMQ.Client
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net.WebSockets;
    using System.Threading;

    using MiniMQ.Client.Implementation;
    using MiniMQ.Client.Model;

    public class Client : IClient
    {
        private ConcurrentDictionary<ReactiveClientConnection, bool> clients = new ConcurrentDictionary<ReactiveClientConnection, bool>();

        public async Task<IClientConnection> ConnectToServer(Uri serverUri, IReactiveConnection reactiveConnection, CancellationToken cancellationToken)
        {
            var client = new ClientWebSocket();
            client.Options.KeepAliveInterval = TimeSpan.FromSeconds(120);

            await client.ConnectAsync(serverUri, cancellationToken);
            var connection = new ReactiveClientConnection(client, reactiveConnection, cancellationToken);

            await reactiveConnection.OnInitialize(connection);
            connection.StartReceivingNewMessage();

            this.clients.TryAdd(connection, false);

            return connection;
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
