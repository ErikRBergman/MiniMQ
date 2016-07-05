namespace MiniMQ.Client
{
    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IClientConnection
    {
        Task CloseAsync();

        Task SendAsync(ArraySegment<byte> data, MessageType messageType, bool endOfMessage, CancellationToken cancellationToken);

        Task<ReceiveResult> ReceiveAsync(ArraySegment<byte> data, CancellationToken cancellationToken);

        Stream GetOutputStream(bool bufferingStream = true);

        Stream GetInputStream(bool bufferingStream = true);

    }
}