namespace SampleClient
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Client;

    internal static class Program
    {
        internal static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 200;

            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var httpServerBaseUrl = ConfigurationManager.AppSettings["serverBaseUrl"];
            httpServerBaseUrl = NormalizeUrl(httpServerBaseUrl);

            var webSocketServerBaseUrl = ConfigurationManager.AppSettings["webSocketServerBaseUrl"];
            webSocketServerBaseUrl = NormalizeUrl(webSocketServerBaseUrl);

            int clientCount = 50;

            var applicationName = "app1";


            await Task.Delay(3000);

            await RunRawClients(httpServerBaseUrl, applicationName, clientCount);
            // await RunSocketClients(webSocketServerBaseUrl, applicationName, clientCount);
        }

        private static string NormalizeUrl(string httpServerBaseUrl)
        {
            if (httpServerBaseUrl.EndsWith("/") == false)
            {
                httpServerBaseUrl = httpServerBaseUrl + "/";
            }
            return httpServerBaseUrl;
        }
        
        private static Task RunSocketClients(string baseUrl, string applicationName, int clientCount)
        {
            var tasks = new List<Task>(clientCount);
            var client = new Client();

            for (var i = 0; i < clientCount; i++)
            {
                tasks.Add(RunSocketClient(client, baseUrl, applicationName));
            }

            return Task.WhenAll(tasks);
        }

        private static async Task RunSocketClient(
            Client client, 
            string baseUrl,
            string applicationName)
        {
            await Task.Yield();
            await client.ConnectToServer(new Uri(baseUrl + "wsc/" + applicationName), new ReactiveClient(), CancellationToken.None);
        }

        private static Task RunRawClients(string baseUrl, string applicationName, int clientCount)
        {
            return RawMiniMqClientHttpClient.RunRawClients(clientCount, applicationName, baseUrl);
        }
    }
}
