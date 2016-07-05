// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageQueue.cs" company="">
//   
// </copyright>
// <summary>
//   The message queue.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MiniMQ.Core.MessageHandlers.InMemory.Queue
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    /// <summary>
    /// The message queue.
    /// </summary>
    internal class InMemoryMessageQueue : IMessageHandler
    {
        private readonly IMessageFactory messageFactory;

        /// <summary>
        /// The messages.
        /// </summary>
        private readonly ConcurrentQueue<IMessage> messages = new ConcurrentQueue<IMessage>();

        /// <summary>
        /// The semaphore.
        /// </summary>
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);

        public string Name { get; }

        /// <summary>
        /// The can send and receive message.
        /// </summary>
        public bool SupportsSendAndReceiveMessage => false;

        public bool SupportsWebSocketConnections => true;

        public InMemoryMessageQueue(IMessageFactory messageFactory, string name)
        {
            this.Name = name;
            this.messageFactory = messageFactory;
        }

        public IMessageFactory GetMessageFactory()
        {
            return this.messageFactory;
        }

        /// <summary>
        /// The receive message async.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task ReceiveMessageAsync(IMessagePipeline pipeline, CancellationToken cancellationToken)
        {
            await this.semaphore.WaitAsync(cancellationToken);
            IMessage message;

            // this will only fail if there is a bug in the program
            this.messages.TryDequeue(out message);
            await pipeline.SendMessage(message);
        }

        /// <summary>
        /// The receive message or null.
        /// </summary>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public IMessage ReceiveMessageOrNull()
        {
            IMessage message;

            if (this.messages.TryDequeue(out message))
            {
                // We will never wait, since there must be an available spot
                this.semaphore.Wait();
                return message;
            }

            return null;
        }

        public Task SendAndReceiveMessageAsync(IMessage message, IMessagePipeline pipeline, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Message queues do not support send and receive");
        }

        /// <summary>
        /// The send and receive message async.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public Task<IMessage> SendAndReceiveMessageAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The send message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task SendMessage(IMessage message)
        {
            this.messages.Enqueue(message);
            this.semaphore.Release(1);
            return Task.CompletedTask;
        }

        /// <summary>
        /// The create stream.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        private static Stream CreateStream(string message)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(message);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

    }
}