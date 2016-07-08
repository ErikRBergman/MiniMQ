namespace SampleClient
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    class RawMiniMqClientHttpClient
    {
        public static string ReceiveMessageWaitUrl(string baseUrl, string messageHandlerName)
        {
            return baseUrl + "rcw/" + messageHandlerName;
        }

        public static string SendMessageUrl(string baseUrl, string messageHandlerName)
        {
            return baseUrl + "snd/" + messageHandlerName;
        }

        public static string SendAndReceiveMessageWaitUrl(string baseUrl, string messageHandlerName)
        {
            return baseUrl + "srw/" + messageHandlerName;
        }

        public static async Task CallAsync(string baseUrl, string applicationName)
        {
            await Task.Yield();

            while (true)
            {
                var request = GetHttpRequest(SendAndReceiveMessageWaitUrl(baseUrl, applicationName));
                request.Method = "POST";
                request.Timeout = 30 * 1000;

                string requestText = "Request at " + DateTime.Now;

                // Console.WriteLine(requestText);

                var bytes = Encoding.UTF8.GetBytes(requestText);
                request.ContentLength = bytes.Length;

                var requestStream = await request.GetRequestStreamAsync();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

                using (var response = await request.GetResponseAsync())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        var reader = new StreamReader(stream);
                        var text = await reader.ReadToEndAsync();

                        if (text != "OK " + requestText)
                        {
                            throw new Exception("Did not ping back...");
                        }

                        Console.Write(".");

                        // Console.WriteLine("Received: " + text);
                    }
                }
            }
        }

        private static HttpWebRequest GetHttpRequest(string url)
        {
            var request = WebRequest.CreateHttp(url);
            request.Credentials = CredentialCache.DefaultCredentials;
            request.KeepAlive = true;
            return request;
        }

        public static Task RunRawClients(int clientCount, string applicationName, string baseUrl)
        {
            var tasks = new List<Task>(clientCount);

            for (int i = 0; i < tasks.Capacity; i++)
            {
                tasks.Add(RawMiniMqClientHttpClient.CallAsync(baseUrl, applicationName));
            }

            return Task.WhenAll(tasks);
        }
    }
}
