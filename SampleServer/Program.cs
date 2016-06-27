using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleServer
{
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading;

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

            string applicationName = "app1";
            CreateApplication(baseUrl, applicationName).Wait();

            var tasks = new List<Task>(50);

            for (int i = 0; i < tasks.Capacity; i++)
            {
                tasks.Add(ServeAsync(baseUrl, applicationName));
            }

            Task.WhenAll(tasks).Wait();
        }

        static string ReceiveMessageWaitUrl(string baseUrl, string messageHandlerName)
        {
            return baseUrl + "rcw/" + messageHandlerName;
        }

        static string CreateApplicationUrl(string baseUrl, string messageHandlerName)
        {
            return baseUrl + "cre_a/" + messageHandlerName;
        }


        static string SendMessageUrl(string baseUrl, string messageHandlerName)
        {
            return baseUrl + "snd/" + messageHandlerName;
        }

        private static long servedCount = 0;

        static async Task ServeAsync(string baseUrl, string applicationName)
        {
            await Task.Delay(5000);
            await Task.Yield();

            Stopwatch stopwatch = null;

            Console.WriteLine("Waiting to serve...");

            while (true)
            {
                var request = GetHttpRequest(ReceiveMessageWaitUrl(baseUrl, applicationName));
                request.Timeout = 1000 * 3600;

                string transactionId;

                string requestText = string.Empty;

                using (var response = await request.GetResponseAsync())
                {
                    if (stopwatch == null)
                    {
                        stopwatch = Stopwatch.StartNew();
                    }

                    transactionId = response.Headers["transactionId"];

                    using (var stream = response.GetResponseStream())
                    {
                        var reader = new StreamReader(stream);
                        requestText = reader.ReadToEnd();

                        // Console.WriteLine("Received: " + text);
                    }
                }

                request = GetHttpRequest(SendMessageUrl(baseUrl, applicationName));
                request.Headers["transactionId"] = transactionId;
                request.Method = "POST";
                request.Timeout = 30 * 1000;

                var responseText = "OK " + requestText;
                var bytes = Encoding.UTF8.GetBytes(responseText);
                request.ContentLength = bytes.Length;

                var requestStream = await request.GetRequestStreamAsync();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

                using (var response = await request.GetResponseAsync())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        //var reader = new StreamReader(stream);
                        //var text = reader.ReadToEnd();

                        // Console.WriteLine("Received: " + text);
                    }
                }

                var count = Interlocked.Increment(ref servedCount);

                if (count % 10000 == 0)
                { 
                    Console.WriteLine(count + " served after " + stopwatch.Elapsed);
                }

            }


        }

        private static async Task CreateApplication(string baseUrl, string applicationName)
        {
            var request = GetHttpRequest(CreateApplicationUrl(baseUrl, applicationName));
            request.Timeout = 1000 * 30;

            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    var text = reader.ReadToEnd();

                    Console.WriteLine("Application created: " + text);
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
