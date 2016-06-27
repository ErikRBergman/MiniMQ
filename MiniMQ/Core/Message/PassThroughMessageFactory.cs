using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiniMQ.Core.Message
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class PassThroughMessageFactory : IMessageFactory
    {
        public Task<IMessage> CreateMessage(Stream stream)
        {
            return Task.FromResult((IMessage)new Message(stream));
        }

        public Task<IMessage> CreateMessage(Stream stream, string uniqueId)
        {
            return Task.FromResult((IMessage)new Message(stream, uniqueId));
        }

        public Task<IMessage> CreateMessage(string text)
        {
            return Task.FromResult((IMessage)new Message(new MemoryStream(Encoding.UTF8.GetBytes(text))));
        }
    }
}