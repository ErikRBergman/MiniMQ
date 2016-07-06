namespace MiniMQ.Core.MessageHandlers.InMemory.Queue
{
    public interface IWebSocketSubscriber
    {
        void Cancel();

        void Subscribe();
    }
}