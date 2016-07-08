namespace SampleServer
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 200;

            var baseUrl = ConfigurationManager.AppSettings["serverBaseUrl"].WithEndingSlash();
            var reactiveBaseUrl = ConfigurationManager.AppSettings["reactiveServerBaseUrl"].WithEndingSlash();

            int serviceCount = 1;
            string applicationName = "app1";

            MainAsync(baseUrl, reactiveBaseUrl, applicationName, serviceCount).Wait();

            Console.ReadKey(false);
        }


        private static async Task MainAsync(string baseUrl, string reactiveBaseUrl, string applicationName, int serviceCount)
        {
            await Task.Delay(2000);
            await RawApplicationServer.CreateApplication(baseUrl, applicationName);

            var client = new ClientWebSocket();
            client.Options.KeepAliveInterval = TimeSpan.FromSeconds(120);
            await client.ConnectAsync(new Uri(reactiveBaseUrl + "wsc/" + applicationName), CancellationToken.None);

            var buffer = new byte[1024];

            Console.WriteLine("Connected to websocket");

//            await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Run to chuck")), WebSocketMessageType.Text, true, CancellationToken.None);

            //while (client.State == WebSocketState.Open)
            //{
            //    var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            //    if (result.MessageType == WebSocketMessageType.Close)
            //    {
            //        Console.WriteLine("Socket closed!");
            //        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Requested by caller", CancellationToken.None);
            //        return;
            //    }

            //    var text = Encoding.UTF8.GetString(buffer, 0, result.Count);

            //    Console.WriteLine(text);

            //    await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("OK fred, " + text)), WebSocketMessageType.Text, true, CancellationToken.None);
            //}

            //Console.WriteLine("Socket not open anymore...");

            //return;

            var tasks = new List<Task>(serviceCount+1);

            var reactiveApplicationServer = new ReactiveApplicationServer();

            for (int i = 0; i < serviceCount; i++)
            {

                tasks.Add(reactiveApplicationServer.ServeAsync(reactiveBaseUrl, applicationName, CancellationToken.None));

//                tasks.Add(RawApplicationServer.ServeAsync(baseUrl, applicationName));
            }

            // Wait for all eternity
            await new TaskCompletionSource<bool>().Task;

            await Task.WhenAll(tasks);
        }

    }
}
