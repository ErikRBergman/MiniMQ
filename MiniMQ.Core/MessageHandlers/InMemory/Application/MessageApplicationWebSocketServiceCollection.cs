// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageApplicationWebSocketServiceCollection.cs" company="EB">
//   EB
// </copyright>
// <summary>
//   Defines the MessageApplicationWebSocketServiceCollection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MiniMQ.Core.MessageHandlers.InMemory.Application
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Model.Core.Message;

    using Model.Core.MessageHandler;

    /// <summary>
    /// The message application web socket service collection.
    /// </summary>
    internal class MessageApplicationWebSocketServiceCollection
    {
        /// <summary>
        /// The message handler.
        /// </summary>
        private readonly IMessageHandler messageHandler;

        /// <summary>
        /// The available services.
        /// </summary>
        private readonly ConcurrentQueue<Service> availableServices = new ConcurrentQueue<Service>();

        public MessageApplicationWebSocketServiceCollection(IMessageHandler messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        public IWebSocketClient GetAvailableService()
        {
            Service service;

            while (this.availableServices.TryDequeue(out service))
            {
                service.StopService();
                if (service.WebSocketClient.IsConnected)
                {
                    return service.WebSocketClient;
                }
            }

            return null;
        }

        /// <summary>
        /// The add service async.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        public Task RunServiceAsync(IWebSocketClient client)
        {
            if (client.IsConnected)
            {
                var service = new Service(client, this.messageHandler);
                this.availableServices.Enqueue(service);
                return service.RunServiceAsync();
            }

            return Task.CompletedTask;
        }

        private class Service
        {
            public readonly IMessageHandler messageHandler;

            public Service(IWebSocketClient webSocketClient, IMessageHandler messageHandler)
            {
                this.WebSocketClient = webSocketClient;
                this.messageHandler = messageHandler;
                this.CancellationTokenSource = new CancellationTokenSource();
            }

            public IWebSocketClient WebSocketClient { get; }

            public CancellationTokenSource CancellationTokenSource { get; }

            public async Task RunServiceAsync()
            {
                do
                {
                    //// Get message from handler, send to socket, get from socket, send to handler...

                    var requestMessage = await this.GetMessageFromMessageHandlerAsync();
                    if (requestMessage == null)
                    {
                        return; 
                    }

                    if (await this.SendMessageToSocket(requestMessage) == false)
                    {
                        continue;
                    }

                    var responseMessage = await this.GetMessageFromSocket(requestMessage.UniqueIdentifier);

                    if (responseMessage != null)
                    {
                        await this.messageHandler.SendMessageAsync(responseMessage);
                    }

                }
                while (true);


            }

            private async Task<IMessage> GetMessageFromSocket(string uniqueIdentifier)
            {
                var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.CancellationTokenSource.Token);

                var messageTask = this.WebSocketClient.ReceiveMessageAsync(null, uniqueIdentifier, cancellationTokenSource.Token);
                var disconnectTask = this.WebSocketClient.WaitDisconnectAsync(-1, cancellationTokenSource.Token);

                var finishedTask = await Task.WhenAny(messageTask, disconnectTask);
                var isDisconnected = this.WebSocketClient.IsConnected == false;

                cancellationTokenSource.Cancel();

                if (isDisconnected)
                {
                    if (messageTask.IsCompleted)
                    {
                        // (She wrote upon it) Return to sender... adress unknown... 
                        await this.messageHandler.SendMessageAsync(messageTask.Result);
                    }

                    return null;
                }

                return messageTask.Result;

            }

            private async Task<bool> SendMessageToSocket(IMessage message)
            {
                var sendResult = await this.WebSocketClient.SendMessageAsync(message, this.CancellationTokenSource.Token);

                if (!sendResult)
                {
                    // Failed to send, return to sender and continue working
                    await this.messageHandler.SendMessageAsync(message);
                    return false;
                }

                return true;
            }

            private async Task<IMessage> GetMessageFromMessageHandlerAsync()
            {
                var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.CancellationTokenSource.Token);

                var messageTask = this.messageHandler.ReceiveMessageAsync(null, cancellationTokenSource.Token);
                var disconnectTask = this.WebSocketClient.WaitDisconnectAsync(-1, cancellationTokenSource.Token);

                var finishedTask = await Task.WhenAny(messageTask, disconnectTask);
                var isDisconnected = this.WebSocketClient.IsConnected == false;

                cancellationTokenSource.Cancel();

                if (isDisconnected)
                {
                    if (messageTask.IsCompleted && messageTask.IsCanceled == false)
                    {
                        // (She wrote upon it) Return to sender... adress unknown... 
                        await this.messageHandler.SendMessageAsync(messageTask.Result);
                    }

                    return null;
                }

                return messageTask.Result;
            }

            public void StopService()
            {
                this.CancellationTokenSource.Cancel();
            }
        }


    }
}
