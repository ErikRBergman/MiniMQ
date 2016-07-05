namespace MiniMQ.Client
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IReactiveConnection
    {
        Task OnInitialize(IClientConnection connection);

        Task OnMessageReceived(Stream message);

        void OnConnectionClosed(CloseStatus closeStatus, string closeStatusDescription);
    }
}