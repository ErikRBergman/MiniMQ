namespace MiniMQ.Client
{
    using System;
    using System.Net.WebSockets;

    internal static class MessageTypeConverter
    {
        internal static WebSocketMessageType ConvertToWebSocketMessageType(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Binary:
                    return WebSocketMessageType.Binary;
                case MessageType.Close:
                    return WebSocketMessageType.Close;
                case MessageType.Text:
                    return WebSocketMessageType.Text;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }

        internal static MessageType ConvertToMessageType(WebSocketMessageType messageType)
        {
            switch (messageType)
            {
                case WebSocketMessageType.Text:
                    return MessageType.Text;
                case WebSocketMessageType.Binary:
                    return MessageType.Binary;
                case WebSocketMessageType.Close:
                    return MessageType.Close;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }
    }
}