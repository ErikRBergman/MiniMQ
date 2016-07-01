namespace MiniMQ.Core.MessageHandler
{
    using System.Threading.Tasks;

    public interface IMessageHandlerFactory
    {
        Task<IMessageHandler> CreateQueue(string queueName);

        Task<IMessageHandler> CreateBus(string busName);

        Task<IMessageHandler> CreateApplication(string applicationName);

    }
}