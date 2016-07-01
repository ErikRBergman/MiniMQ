namespace MiniMQ.Routing
{
    using System.Threading.Tasks;
    using System.Web;

    using MiniMQ.Core.Message;

    public struct HttpContextResponseOutputMessagePipeline : IMessagePipeline
    {
        private readonly HttpResponse httpResponse;

        public HttpContextResponseOutputMessagePipeline(HttpResponse httpResponse)
        {
            this.httpResponse = httpResponse;
        }

        public async Task SendMessage(IMessage message)
        {
            if (this.httpResponse.IsClientConnected == false)
            {
                return;
            }

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