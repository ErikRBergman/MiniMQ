namespace MiniMQ.Client
{
    using System;
    using System.Net.WebSockets;

    static internal class ResultConverter
    {
        internal static ReceiveResult ConvertToRecieveResult(WebSocketReceiveResult webSocketReceiveResult)
        {
            return new ReceiveResult(webSocketReceiveResult.Count, MessageTypeConverter.ConvertToMessageType(webSocketReceiveResult.MessageType), webSocketReceiveResult.EndOfMessage);
        }
    }
}

