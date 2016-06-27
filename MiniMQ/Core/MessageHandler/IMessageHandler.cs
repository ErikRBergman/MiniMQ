// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The MessageHandler interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MiniMQ.Core.MessageHandler
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Core.Message;

    /// <summary>
    /// The MessageHandler interface.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Gets a value indicating whether can send and receive message.
        /// </summary>
        bool CanSendAndReceiveMessage { get; }

        /// <summary>
        /// The get message factory.
        /// </summary>
        /// <returns>
        /// The <see cref="IMessageFactory"/>.
        /// </returns>
        IMessageFactory GetMessageFactory();

        /// <summary>
        /// The receive message async.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken);

        /// <summary>
        /// The receive message or null.
        /// </summary>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        IMessage ReceiveMessageOrNull();

        /// <summary>
        /// The send and receive message async.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<IMessage> SendAndReceiveMessageAsync(IMessage message, CancellationToken cancellationToken);

        /// <summary>
        /// The send message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task SendMessage(IMessage message);
    }
}