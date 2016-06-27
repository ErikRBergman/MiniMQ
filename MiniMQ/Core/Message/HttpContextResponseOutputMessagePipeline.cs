using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiniMQ.Core.Message
{
    using System.IO;
    using System.Threading.Tasks;

    public struct HttpContextResponseOutputMessagePipeline : IMessagePipeline
    {
        private readonly HttpResponse httpResponse;

        public HttpContextResponseOutputMessagePipeline(HttpResponse httpResponse)
        {
            this.httpResponse = httpResponse;
        }

        public async Task SendMessage(IMessage message)
        {
            var stream = await message.GetStream();

            if (stream != null)
            {
                this.httpResponse.Headers["transactionId"] = message.UniqueIdentifier;

                using (stream)
                {
                    stream.Position = 0;
                    await stream.CopyToAsync(this.httpResponse.OutputStream);
                }
            }
        }
    }
}