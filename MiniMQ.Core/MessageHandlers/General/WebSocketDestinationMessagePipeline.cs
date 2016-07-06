namespace MiniMQ.Core.MessageHandlers.General
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    internal struct WebSocketDestinationMessagePipeline: IMessagePipeline
    {
        private readonly IWebSocketClient webSocketClient;
        private readonly IMessageHandler messageHandler;

        public WebSocketDestinationMessagePipeline(IMessageHandler messageHandler, IWebSocketClient webSocketClient)
        {
            this.messageHandler = messageHandler;
            this.webSocketClient = webSocketClient;
        }

        public async Task<bool> SendMessageAsync(IMessage message)
        {
            try
            {
                if (this.webSocketClient.IsConnected)
                {
                    await this.webSocketClient.SendMessageAsync(message, CancellationToken.None);
                    return true;
                }
            }
            catch (Exception)
            {
                // Error in message delivery, return to the queue
                await this.messageHandler.SendMessageAsync(message);
                throw;
            }

            // Client is no longer connected - post the message back to the queue
            await this.messageHandler.SendMessageAsync(message);


            // TODO: Figure out if this is really true and if we should remove the line sending the message back to the queue
            return false;
        }
    }
}