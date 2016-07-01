namespace MiniMQ.Core.Routing
{
    public enum PathAction
    {
        // Unknown
        Unknown,

        // Queues - FIFO
        CreateQueue,

        // Applications - fire or forget, or fire and wait for answer
        CreateApplication,

        // Buses - Send once and all receivers get it. 
        // Unregistred receivers get all messages while connected
        // Registred receivers will make the bus store messages until all registred receivers have received them
        CreateBus,

        // q, a, & b
        SendMessage,

        // q, a, & b
        ReceiveMessage,

        // q, a, & b
        ReceiveMessageWait,

        // For applications and buses
        SendAndReceiveMessageWait,

    }
}