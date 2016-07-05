namespace MiniMQ.Model.Core.MessageHandler
{
    public interface IMessageHandlerContainer
    {
        IMessageHandler GetMessageHandler(string name);

        bool AddMessageHandler(string name, IMessageHandler messageHandler);

        bool ContainsMessageHandler(string name);

    }
}