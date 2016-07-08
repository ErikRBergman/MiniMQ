namespace MiniMQ.Core.Core.Stream
{
    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class StreamExtensions
    {
        public static async Task CopyToAsync(this System.IO.Stream stream, WebSocket webSocket, CancellationToken cancellationToken)
        {
            if (stream.CanSeek)
            {
                await CopyToSeekAsync(stream, webSocket, cancellationToken);
                return;
            }

            await CopyToUnseekdAsync(stream, webSocket, cancellationToken);
        }

        private static async Task CopyToSeekAsync(Stream stream, WebSocket webSocket, CancellationToken cancellationToken)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var bytesLeft = stream.Length;

            byte[] buffer = new byte[2 * 1024];

            while (bytesLeft > 0)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                bytesLeft -= bytesRead;

                if (bytesRead > 0)
                {
                    var endOfMessage = bytesLeft == 0;
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), WebSocketMessageType.Binary, endOfMessage, cancellationToken);
                }
            }
        }

        private static async Task CopyToUnseekdAsync(Stream stream, WebSocket webSocket, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[2 * 1024];

            int bufferUsageLength = 0;

            do
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                if (bytesRead == 0)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bufferUsageLength), WebSocketMessageType.Binary, true, cancellationToken);
                    return;
                }

                bufferUsageLength += bytesRead;
                bytesRead = await stream.ReadAsync(buffer, bufferUsageLength, buffer.Length - bufferUsageLength, cancellationToken);
                var endOfMessage = bytesRead == 0;

                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), WebSocketMessageType.Binary, endOfMessage, cancellationToken);
            }
            while (true);
        }
    }
}
