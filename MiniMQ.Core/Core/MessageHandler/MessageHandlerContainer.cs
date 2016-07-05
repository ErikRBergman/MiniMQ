namespace MiniMQ.Core.MessageHandler
{
    using System;
    using System.Collections.Concurrent;

    using MiniMQ.Model.Core.MessageHandler;

    public class MessageHandlerContainer : IMessageHandlerContainer
    {
        /// <summary>
        /// The message handlers. The names are case sensitive
        /// </summary>
        private readonly ConcurrentDictionary<string, IMessageHandler> messageHandlers = new ConcurrentDictionary<string, IMessageHandler>(StringComparer.Ordinal);

        public IMessageHandler GetMessageHandler(string name)
        {
            IMessageHandler messageHandler;

            if (this.messageHandlers.TryGetValue(name, out messageHandler))
            {
                return messageHandler;
            }

            return null;
        }

        public bool AddMessageHandler(string name, IMessageHandler messageHandler)
        {
            return this.messageHandlers.TryAdd(name, messageHandler);
        }

        public bool ContainsMessageHandler(string name)
        {
            return this.messageHandlers.ContainsKey(name);
        }

    }
}