using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Core.MessageHandlers.General
{
    using System.Collections.Concurrent;
    using System.Threading;

    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    internal class WebSocketCollection
    {
        private ConcurrentDictionary<IWebSocketClient, IWebSocketClient> webSocketClients = new ConcurrentDictionary<IWebSocketClient, IWebSocketClient>();

        public void Add(IWebSocketClient client)
        {
            this.webSocketClients.TryAdd(client, client);
        }

        public void Remove(IWebSocketClient client)
        {
            IWebSocketClient existing;
            this.webSocketClients.TryRemove(client, out existing);
        }



        public Task<bool[]> Broadcast(IMessage message, CancellationToken cancellationToken)
        {
            var tasks = new List<Task<bool>>(this.webSocketClients.Count);

            foreach (var client in this.webSocketClients.Values)
            {
                if (client.IsConnected == false)
                {
                    this.Remove(client);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                tasks.Add(Task.Run(() => client.SendMessageAsync(message, cancellationToken), cancellationToken).ContinueWith(t => t.IsCompleted));
            }

            return Task.WhenAll(tasks);
        }  

    }
}
