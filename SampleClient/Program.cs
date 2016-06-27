using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleServer
{
    using System.Configuration;
    using System.IO;
    using System.Net;

    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 200;

            var baseUrl = ConfigurationManager.AppSettings["serverBaseUrl"];
            if (baseUrl.EndsWith("/") == false)
            {
                baseUrl = baseUrl + "/";
            }

            var tasks = new List<Task>(50);

            for (int i = 0; i < tasks.Capacity; i++)
            {
                tasks.Add(CallAsync(baseUrl, "app1"));
            }

            Task.WhenAll(tasks).Wait();

        }

        static string ReceiveMessageWaitUrl(string baseUrl, string messageHandlerName)
        {
            return baseUrl + "rcw/" + messageHandlerName;
        }

        static string SendMessageUrl(string baseUrl, string messageHandlerName)
        {
            return baseUrl + "snd/" + messageHandlerName;
        }

        static string SendAndReceiveMessageWaitUrl(string baseUrl, string messageHandlerName)
        {
            return baseUrl + "srw/" + messageHandlerName;
        }


        static async Task CallAsync(string baseUrl, string applicationName)
        {
            await Task.Delay(8000);

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
                        var text = reader.ReadToEnd();

                        if (text != "OK " + requestText)
                        {
                            throw new Exception("Did not ping back...");
                        }

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
    }
}
