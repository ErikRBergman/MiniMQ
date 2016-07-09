namespace SampleServer
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Client;
    using MiniMQ.Client.Implementation;

    internal class ReactiveApplicationServerConnection : IReactiveConnection
    {
        private IClientConnection connection;

        private static int servedCount = 0;

        private static Stopwatch stopwatch = new Stopwatch();

        public Task OnConnectAsync(IClientConnection connection)
        {
            stopwatch.Start();

            Console.WriteLine("Connected to web socket...");

            this.connection = connection;
            return Task.CompletedTask;
        }

        public async Task OnMessageReceivedAsync(Stream message)
        {
            string requestText;

            using (var reader = new StreamReader(message))
            {
                requestText = await reader.ReadToEndAsync();
            }

            // Console.Write(".");
            var count = Interlocked.Increment(ref servedCount);

            if (count % 10000 == 0)
            {
                Console.WriteLine(count + " served after " + stopwatch.Elapsed);
            }


            // Console.WriteLine("Received message: " + requestText);

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
