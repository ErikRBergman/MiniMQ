namespace MiniMQ.Model.Core.Message
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IMessageFactory
    {
        Task<IMessage> CreateMessage(Stream stream);

        Task<IMessage> CreateMessage(Stream stream, string uniqueId);

        Task<IMessage> CreateMessage(string text);
    }
}
