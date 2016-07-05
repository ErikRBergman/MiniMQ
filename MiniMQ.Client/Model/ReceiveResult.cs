namespace MiniMQ.Client
{
    using System;

    public class ReceiveResult
    {
        /// <summary>Indicates the number of bytes that the WebSocket received.</summary>
        /// <returns>Returns <see cref="T:System.Int32" />.</returns>
        public int Count { get; private set; }

        /// <summary>Indicates whether the message has been received completely.</summary>
        /// <returns>Returns <see cref="T:System.Boolean" />.</returns>
        public bool IsEndOfMessage { get; private set; }

        /// <summary>Indicates whether the current message is a UTF-8 message or a binary message.</summary>
        /// <returns>Returns <see cref="T:System.Net.WebSockets.WebSocketMessageType" />.</returns>
        public MessageType MessageType { get; private set; }

        /// <summary>Indicates the reason why the remote endpoint initiated the close handshake.</summary>
        /// <returns>Returns <see cref="T:System.Net.WebSockets.WebSocketCloseStatus" />.</returns>
        public CloseStatus? CloseStatus { get; private set; }

        /// <summary>Returns the optional description that describes why the close handshake has been initiated by the remote endpoint.</summary>
        /// <returns>Returns <see cref="T:System.String" />.</returns>
        public string CloseStatusDescription { get; private set; }

        /// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketReceiveResult" /> class.</summary>
        /// <param name="count">The number of bytes received.</param>
        /// <param name="messageType">The type of message that was received.</param>
        /// <param name="endOfMessage">Indicates whether this is the final message.</param>
        public ReceiveResult(int count, MessageType messageType, bool endOfMessage)
            : this(count, messageType, endOfMessage, new CloseStatus?(), (string)null)
        {
        }

        /// <summary>Creates an instance of the <see cref="T:System.Net.WebSockets.WebSocketReceiveResult" /> class.</summary>
        /// <param name="count">The number of bytes received.</param>
        /// <param name="messageType">The type of message that was received.</param>
        /// <param name="isEndOfMessage">Indicates whether this is the final message.</param>
        /// <param name="closeStatus">Indicates the <see cref="T:System.Net.WebSockets.WebSocketCloseStatus" /> of the connection.</param>
        /// <param name="closeStatusDescription">The description of <paramref name="closeStatus" />.</param>
        public ReceiveResult(int count, MessageType messageType, bool isEndOfMessage, CloseStatus? closeStatus, string closeStatusDescription)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            this.Count = count;
            this.IsEndOfMessage = isEndOfMessage;
            this.MessageType = messageType;
            this.CloseStatus = closeStatus;
            this.CloseStatusDescription = closeStatusDescription;
        }
    }
}