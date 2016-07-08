namespace MiniMQ.Client.Implementation
{
    internal interface IReactiveClientConnection : IClientConnection
    {
        void StartReceivingNewMessage();

        void StopReceivingMessages();
    }
}