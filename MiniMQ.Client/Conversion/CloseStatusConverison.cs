using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Client.Conversion
{
    using System.Net.WebSockets;

    internal static class CloseStatusConverison
    {
        internal static CloseStatus ConvertToCloseStatus(WebSocketCloseStatus webSocketCloseStatus)
        {
            return (CloseStatus)webSocketCloseStatus;
        }
    }
}
