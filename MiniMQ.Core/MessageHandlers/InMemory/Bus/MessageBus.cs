namespace MiniMQ.Core.MessageHandlers.InMemory.Bus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Model.Core.Message;
    using Model.Core.MessageHandler;

    public class MessageBus : IMessageHandler
    {
        public string Name { get; }

        public bool SupportsSendAndReceiveMessage { get; }

        public bool SupportsWebSocketConnections => true;

        public IMessageFactory MessageFactory { get; }

        public Task<IMessage> ReceiveMessageAsync(IMessagePipeline pipeline, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IMessage ReceiveMessageOrNull()
        {
            throw new NotImplementedException();
        }

        public Task<IMessage> SendAndReceiveMessageAsync(IMessage message, IMessagePipeline returnMessagePipeline, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SendMessageAsync(IMessage message)
        {
            throw new NotImplementedException();
        }

        public Task RegisterWebSocket(IWebSocketClient webSocketClient)
        {
            throw new NotImplementedException();
        }
    }
}
