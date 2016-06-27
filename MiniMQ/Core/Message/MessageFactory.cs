namespace MiniMQ.Core.Message
{
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

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
            var newStream = GetNewStream((int)message.Length);

            message.Position = 0;
            await message.CopyToAsync(newStream);
            return newStream;
        }

        public static ObjectPool<PooledStream> MemoryStreamPool = new ObjectPool<PooledStream>(2000);

        private static Stream GetNewStream(int minCapacity)
        {
            var stream = MemoryStreamPool.GetNewObject(() => new PooledStream(new MemoryStream(minCapacity), MemoryStreamPool));

            stream.Position = 0;

            if (stream.Capacity < minCapacity)
            {
                stream.Capacity = minCapacity;
            }

            return stream;
        }
    }
}