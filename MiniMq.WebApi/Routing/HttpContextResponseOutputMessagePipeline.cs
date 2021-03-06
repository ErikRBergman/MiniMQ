﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiniMQ.Core.Message
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    public struct HttpContextResponseOutputMessagePipeline : IMessagePipeline
    {
        private readonly HttpContext httpContext;

        public HttpContextResponseOutputMessagePipeline(HttpContext httpContext)
        {
            this.httpContext = httpContext;
        }

        public async Task<bool> SendMessageAsync(IMessage message)
        {
            if (this.httpContext.RequestAborted.IsCancellationRequested)
            {
                return false;
            }

            var stream = await message.GetStream();

            if (stream != null)
            {
                this.httpContext.Request.Headers["transactionId"] = message.UniqueIdentifier;

                using (stream)
                {
                    if (stream.CanSeek)
                    {
                        stream.Position = 0;
                    }

                    await stream.CopyToAsync(this.httpContext.Response.Body);
                }
            }

            return true;
        }
    }
}