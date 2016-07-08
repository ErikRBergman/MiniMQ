namespace SampleServer
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Client;
    using MiniMQ.Client.Implementation;

    internal class ReactiveApplicationServerConnection : IReactiveConnection
    {
        private IClientConnection connection;

        public Task OnInitialize(IClientConnection connection)
        {
            this.connection = connection;
            return Task.CompletedTask;
        }

        public async Task OnMessageReceived(Stream message)
        {
            string requestText;

            using (var reader = new StreamReader(message))
            {
                requestText = await reader.ReadToEndAsync();
            }

            var responseText = "OK " + requestText;
            var bytes = Encoding.UTF8.GetBytes(responseText);

            await this.connection.SendAsync(bytes.AsArraySegment(), MessageType.Text, true, CancellationToken.None);
        }

        public void OnConnectionClosed(CloseStatus closeStatus, string closeStatusDescription)
        {
            throw new NotImplementedException();
        }
    }
}
