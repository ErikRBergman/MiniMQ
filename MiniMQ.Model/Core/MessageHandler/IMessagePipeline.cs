namespace MiniMQ.Model.Core.MessageHandler
{
    using System.Threading.Tasks;
    using Message;

    public interface IMessagePipeline
    {
        /// <summary>
        /// The send message async.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="Task&lt;boolean&gt;"/> returning true if message was sucessfully sent, and false if not.
        /// </returns>
        Task<bool> SendMessageAsync(IMessage message);
    }
}
