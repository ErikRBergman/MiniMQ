namespace MiniMQ.Core.Message
{
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using MiniMQ.Core.Core.Stream;
    using MiniMQ.Core.Message.Pool;

    public class MessageFactory : IMessageFactory
    {
        public async Task<IMessage> CreateMessage(Stream stream)
        {
            return new Message(await CreateStreamCopy(stream));
        }

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