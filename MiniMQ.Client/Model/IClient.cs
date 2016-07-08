namespace MiniMQ.Client
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Client.Model;

    public interface IClient
    {
        Task<IClientConnection> ConnectToServer(System.Uri serverUri, IReactiveConnection reactiveConnection, CancellationToken cancellationToken);

        Task<IClientConnection> ConnectToServer(System.Uri serverUri, CancellationToken cancellationToken);

        Task<Message> ReceiveMessageOrNullAsync(System.Uri serverUri, CancellationToken cancellationToken);

        Task<Message> ReceiveMessageAsync(System.Uri serverUri, CancellationToken cancellationToken);

        Task<IReadOnlyCollection<Message>> ReceiveAllMessagesAsync(System.Uri serverUri, CancellationToken cancellationToken);

        Task SendMessageAsync(System.Uri serverUri, Message message, CancellationToken cancellationToken);


    }
}