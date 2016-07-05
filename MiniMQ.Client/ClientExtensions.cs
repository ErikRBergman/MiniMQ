namespace MiniMQ.Client
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ClientExtensions
    {
        public static Task<IClientConnection> ConnectToServer(this IClient client, string serverUrl, CancellationToken cancellationToken)
        {
            return client.ConnectToServer(new Uri(serverUrl), cancellationToken);
        }

    }
}