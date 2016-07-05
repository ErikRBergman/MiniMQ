namespace MiniMQ.Client
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ClientConnectionExtensions
    {
        public static Task<ReceiveResult> ReceiveAsync(this IClientConnection connection, ArraySegment<byte> data)
        {
            return connection.ReceiveAsync(data, CancellationToken.None);
        }

        public static Task SendAsync(this IClientConnection connection, ArraySegment<byte> data)
        {
            return connection.SendAsync(data, CancellationToken.None);
        }

        public static Task SendAsync(this IClientConnection connection, string data)
        {
            return connection.SendAsync(data, CancellationToken.None);
        }

        public static Task SendAsync(this IClientConnection connection, ArraySegment<byte> data, CancellationToken cancellationToken)
        {
            return connection.SendAsync(data, MessageType.Binary, cancellationToken);
        }

        public static Task SendAsync(this IClientConnection connection, string data, CancellationToken cancellationToken)
        {
            return connection.SendAsync(data, true, cancellationToken);
        }

        public static Task SendAsync(this IClientConnection connection, string data, bool endOfMessage, CancellationToken cancellationToken)
        {
            return connection.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(data)), MessageType.Text, endOfMessage, cancellationToken);
        }

        public static Task SendAsync(this IClientConnection connection, ArraySegment<byte> data, MessageType messageType, CancellationToken cancellationToken)
        {
            return connection.SendAsync(data, messageType, true, cancellationToken);
        }

        public static Task SendAsync(this IClientConnection connection, byte[] data, MessageType messageType, CancellationToken cancellationToken)
        {
            return connection.SendAsync(data, messageType, true, cancellationToken);
        }

        public static Task SendAsync(this IClientConnection connection, byte[] data, MessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            return connection.SendAsync(new ArraySegment<byte>(data), messageType, endOfMessage, cancellationToken);
        }

    }
}