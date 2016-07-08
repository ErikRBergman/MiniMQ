namespace SampleServer
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class RawApplicationServer
    {
        private static long servedCount = 0;

        private static string ReceiveMessageWaitUrl(string baseUrl, string messageHandlerName)
        {
            return baseUrl + "rcw/" + messageHandlerName;
        }

        private static string CreateApplicationUrl(string baseUrl, string messageHandlerName)
        {
            return baseUrl + "cre_a/" + messageHandlerName;
        }

        private static string SendMessageUrl(string baseUrl, string messageHandlerName)
        {
            return baseUrl + "snd/" + messageHandlerName;
        }

        internal static async Task ServeAsync(string baseUrl, string applicationName)
        {
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
                        if (stream != null)
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                requestText = reader.ReadToEnd();
                            }
                        }

                        // Console.WriteLine("Received: " + text);
                    }
                }

                request = GetHttpRequest(SendMessageUrl(baseUrl, applicationName));
                request.Headers["transactionId"] = transactionId;
                request.Method = "POST";
                request.Timeout = 30 * 1000;

                var responseText = "OK " + requestText;
                var responseRawData = Encoding.UTF8.GetBytes(responseText);
                request.ContentLength = responseRawData.Length;

                var requestStream = await request.GetRequestStreamAsync();
                requestStream.Write(responseRawData, 0, responseRawData.Length);
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

        internal static async Task CreateApplication(string baseUrl, string applicationName)
        {
            var request = GetHttpRequest(CreateApplicationUrl(baseUrl, applicationName));
            request.Timeout = 1000 * 30;

            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var text = await reader.ReadToEndAsync();
                        Console.WriteLine("Application created: " + text);
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