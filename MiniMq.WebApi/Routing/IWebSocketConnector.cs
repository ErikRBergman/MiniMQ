namespace MiniMq.WebApi.Routing
{
    using System.Net.WebSockets;
    using System.Threading.Tasks;

    using MiniMQ.Model.Core.MessageHandler;

    public interface IWebSocketConnector
    {
        /// <summary>
        /// The connect async.
        /// </summary>
        /// <param name="webSocket">
        /// The web socket.
        /// </param>
        /// <param name="messageHandler">
        /// The message handler.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task ConnectAsync(WebSocket webSocket, IMessageHandler messageHandler);
    }
}