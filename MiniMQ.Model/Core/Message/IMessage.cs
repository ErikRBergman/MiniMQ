namespace MiniMQ.Model.Core.Message
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IMessage
    {
        Task<Stream> GetStream();

        string UniqueIdentifier { get; }


    }
}
