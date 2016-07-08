using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Model.Core.MessageHandler
{
    using System.Threading;

    using MiniMQ.Model.Core.Message;

    public interface IWebSocketClient
    {
        /// <summary>
        /// Gets a value indicating whether is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// The send message async.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<bool> SendMessageAsync(IMessage message, CancellationToken cancellationToken);

        /// <summary>
        /// The wait disconnect async.
        /// </summary>
        /// <param name="milliseconds">
        /// The milliseconds.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<bool> WaitDisconnectAsync(int milliseconds, CancellationToken cancellationToken);

        /// <summary>
        /// The receive message async.
        /// </summary>
        /// <param name="messagePipeline">
        /// The message pipeline.
        /// </param>
        /// <param name="messageUniqueIdentifier"></param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<IMessage> ReceiveMessageAsync(IMessagePipeline messagePipeline, string messageUniqueIdentifier, CancellationToken token);
    }

    public static class IWebSocketClientExtensions
    {
        public static Task WaitDisconnectAsync(this IWebSocketClient webSocketClient, CancellationToken cancellationToken)
        {
            return webSocketClient.WaitDisconnectAsync(-1, cancellationToken);
        }

        public static Task WaitDisconnectAsync(this IWebSocketClient webSocketClient)
        {
            return webSocketClient.WaitDisconnectAsync(-1, CancellationToken.None);
        }

    }
}
