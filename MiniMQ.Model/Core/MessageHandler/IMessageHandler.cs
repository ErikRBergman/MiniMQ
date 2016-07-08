// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageHandler.cs" company="EB">
//   EB
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

    using Message;

    /// <summary>
    /// The MessageHandler interface.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Gets the message handler name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether can send and receive message.
        /// </summary>
        bool SupportsSendAndReceiveMessage { get; }

        /// <summary>
        /// Gets a value indicating whether supports web socket connections.
        /// </summary>
        bool SupportsWebSocketConnections { get; }

        /// <summary>
        /// The get message factory.
        /// </summary>
        /// <value>
        ///   The <see cref="IMessageFactory"/>.
        /// </value>
        IMessageFactory MessageFactory { get; }

        /// <summary>
        /// The receive message async.
        /// </summary>
        /// <param name="pipeline">
        /// The pipeline.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> containing the received message.
        /// </returns>
        Task<IMessage> ReceiveMessageAsync(IMessagePipeline pipeline, CancellationToken cancellationToken);

        /// <summary>
        /// The receive message or null.
        /// </summary>
        /// <returns>
        /// The <see cref="IMessage"/>.
        /// </returns>
        IMessage ReceiveMessageOrNull();

        /// <summary>
        /// The send and receive message async.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="returnMessagePipeline">
        /// The return Message Pipeline.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> containing the received message.
        /// </returns>
        Task<IMessage> SendAndReceiveMessageAsync(IMessage message, IMessagePipeline returnMessagePipeline, CancellationToken cancellationToken);

        /// <summary>
        /// The send message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task SendMessageAsync(IMessage message);

        /// <summary>
        /// The register web socket with message handler.
        /// </summary>
        /// <param name="webSocketClient">
        /// The web socket client.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task RegisterWebSocket(IWebSocketClient webSocketClient);
    }
}