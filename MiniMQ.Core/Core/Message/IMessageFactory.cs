using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Core.Message
{
    using System.IO;

    public interface IMessageFactory
    {
        Task<IMessage> CreateMessage(Stream stream);

        Task<IMessage> CreateMessage(Stream stream, string uniqueId);

        Task<IMessage> CreateMessage(string text);
    }
}
