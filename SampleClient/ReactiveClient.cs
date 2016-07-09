namespace SampleClient
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using MiniMQ.Client;

    internal class ReactiveClient : IReactiveConnection
    {
        /// <summary>
        /// The connection.
        /// </summary>
        private IClientConnection connection;

        /// <summary>
        /// The last request.
        /// </summary>
        private string lastRequest;

        /// <summary>
        /// The initialize.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        public async Task OnConnectAsync(IClientConnection connection)
        {
            this.connection = connection;
            await this.SendRequest();
        }

        /// <summary>
        /// The send request.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task SendRequest()
        {
            using (var stream = this.connection.GetOutputStream())
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    this.lastRequest = "Request at " + DateTime.Now;

                    await writer.WriteAsync(this.lastRequest);
                    await writer.FlushAsync();

                    // To send end of message async
                    await stream.WriteAsync(null, 0, 0);
                }
            }
        }

        /// <summary>
        /// The on message received.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public async Task OnMessageReceivedAsync(Stream message)
        {
            using (var reader = new StreamReader(message))
            {
                var textReceived = await reader.ReadToEndAsync();

                if (string.Compare("OK " + this.lastRequest, textReceived, StringComparison.Ordinal) != 0)
                {
                    throw new Exception("Did not ping back...");
                }
            }

            // Trigger new request
            var t = Task.Run(this.SendRequest);
        }

        public void OnConnectionClosed(CloseStatus closeStatus, string closeStatusDescription)
        {
            throw new NotImplementedException();
        }
    }
}