namespace MiniMQ.Model.Core.MessageHandler
{
    using System.Threading.Tasks;

    public interface IMessageHandlerFactory
    {
        Task<IMessageHandler> Create(string name);
    }

    public interface IMessageHandlerProducer
    {
        IMessageHandlerFactory QueueFactory { get; }

        IMessageHandlerFactory BusFactory { get; }

        IMessageHandlerFactory ApplicationFactory { get; }


    }

}