using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Core.Message
{
    using System.IO;

    public interface IMessage
    {
        Task<Stream> GetStream();

        string UniqueIdentifier { get; }


    }
}
