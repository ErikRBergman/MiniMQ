namespace MiniMQ.Model.Core.MessageHandler
{
    using System.Threading.Tasks;

    using MiniMQ.Model.Core.Message;

    public interface IMessagePipeline
    {
        Task SendMessage(IMessage message);
    }
}
