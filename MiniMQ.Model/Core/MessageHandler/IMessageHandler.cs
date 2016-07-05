// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The MessageHandler interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MiniMQ.Model.Core.MessageHandler
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Model.Core.Message;

    /// <summary>
    /// The MessageHandler interface.
    /// </summary>
    public interface IMessageHandler
    {
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether can send and receive message.
        /// </summary>
        bool SupportsSendAndReceiveMessage { get; }

        bool SupportsWebSocketConnections { get; }

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
        Task ReceiveMessageAsync(IMessagePipeline pipeline, CancellationToken cancellationToken);

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
        Task SendAndReceiveMessageAsync(IMessage message, IMessagePipeline pipeline, CancellationToken cancellationToken);

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