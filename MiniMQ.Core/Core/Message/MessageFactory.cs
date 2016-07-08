namespace MiniMQ.Core.Message
{
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using MiniMQ.Core.Core.Stream;
    using MiniMQ.Core.Message.Pool;
    using MiniMQ.Model.Core.Message;

    public class MessageFactory : IMessageFactory
    {
        public async Task<IMessage> CreateMessage(Stream stream)
        {
            return new Message(await CreateStreamCopy(stream));
        }

        /// <summary>
        /// Create a new message from a stream with the unique id.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="uniqueId">
        /// The unique id. If uniqueId is null, a new id will be created.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<IMessage> CreateMessage(Stream stream, string uniqueId)
        {
            return new Message(await CreateStreamCopy(stream), uniqueId);
        }

        public Task<IMessage> CreateMessage(string text)
        {
            return Task.FromResult((IMessage)new Message(new MemoryStream(Encoding.UTF8.GetBytes(text))));
        }

        /// <summary>
        /// The create stream copy.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private static async Task<Stream> CreateStreamCopy(Stream message)
        {
            var newStream = GetNewStream();
            await message.CopyToAsync(newStream);
            newStream.Position = 0;
            return newStream;
        }

        private static Stream GetNewStream()
        {
            return new PartitionedByteStream(true);
        }
    }
}