namespace MiniMQ.Client
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IReactiveConnection
    {
        Task OnConnectAsync(IClientConnection connection);

        Task OnMessageReceivedAsync(Stream message);

        void OnConnectionClosed(CloseStatus closeStatus, string closeStatusDescription);
    }
}