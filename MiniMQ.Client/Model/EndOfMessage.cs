using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Client.Model
{
    using System.IO;

    public static class EndOfMessage
    {
        public static Task SendEndOfMessageAsync(this Stream stream)
        {
            return stream.WriteAsync(Value, 0, 0);
        }

        public static byte[] Value { get; } = new byte[0];
    }
}
