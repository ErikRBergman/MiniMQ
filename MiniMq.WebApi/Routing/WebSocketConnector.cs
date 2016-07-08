using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniMq.WebApi.Routing
{
    using System.Net.WebSockets;

    using MiniMQ.Core.MessageHandlers.General;
    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    public class WebSocketConnector : IWebSocketConnector
    {
        private readonly IHealthChecker healthChecker;

        public WebSocketConnector(IHealthChecker healthChecker)
        {
            this.healthChecker = healthChecker;
        }

        public async Task ConnectAsync(WebSocket webSocket, IMessageHandler messageHandler)
        {
            var client = new WebSocketClient(webSocket, messageHandler.MessageFactory);
            // this.healthChecker.Add(client);
            await messageHandler.RegisterWebSocket(client);
        }
    }
}
