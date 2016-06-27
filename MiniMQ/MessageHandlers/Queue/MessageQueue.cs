// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageQueue.cs" company="">
//   
// </copyright>
// <summary>
//   The message queue.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MiniMQ.MessageHandlers.Queue
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using MiniMQ.Core.Message;
    using MiniMQ.Core.MessageHandler;

    /// <summary>
    /// The message queue.
    /// </summary>
    internal class MessageQueue : IMessageHandler
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

        /// <summary>
        /// The can send and receive message.
        /// </summary>
        public bool CanSendAndReceiveMessage => false;

        public MessageQueue(IMessageFactory messageFactory)
        {
            this.messageFactory = messageFactory;
        }

        public IMessageFactory GetMessageFactory()
        {
            return this.messageFactory;
        }

        /// <summary>
        /// The receive message async.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            await this.semaphore.WaitAsync(cancellationToken);
            IMessage message;

            // this will only fail if there is a bug in the program
            this.messages.TryDequeue(out message);
            return message;
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

        public Task<IMessage> SendAndReceiveMessageAsync(IMessage message, CancellationToken cancellationToken)
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