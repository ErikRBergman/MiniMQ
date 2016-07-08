namespace MiniMQ.Core.MessageHandlers.General
{
    using System.Threading.Tasks;

    using Model.Core.Message;
    using Model.Core.MessageHandler;

    public class MessageSniffingPipeline : IMessagePipeline
    {
        private readonly IMessagePipeline outerMessagePipeline;

        public MessageSniffingPipeline(IMessagePipeline outerMessagePipeline)
        {
            this.outerMessagePipeline = outerMessagePipeline;
        }

        public IMessage Message { get; private set; }

        public Task<bool> SendMessageAsync(IMessage message)
        {
            this.Message = message;
            return this.outerMessagePipeline.SendMessageAsync(message);
        }
    }
}
