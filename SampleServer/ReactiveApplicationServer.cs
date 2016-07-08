using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleServer
{
    using System.Threading;

    using MiniMQ.Client;

    internal class ReactiveApplicationServer
    {
        private readonly Client client = new Client();

        public Task ServeAsync(string baseUrl, string applicationName, CancellationToken cancellationToken)
        {
            return this.client.ConnectToServer(new Uri(baseUrl + "wsc/" + applicationName), new ReactiveApplicationServerConnection(), cancellationToken);
        }
    }
}
