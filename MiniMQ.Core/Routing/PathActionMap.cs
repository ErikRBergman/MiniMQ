namespace MiniMQ.Core.Routing
{
    public static class PathActionMap
    {
        public static PathActionMapItem[] Items { get; } = {
                
                // Admin actions
                new PathActionMapItem("/cre_q/", PathAction.CreateQueue),
                new PathActionMapItem("/cre_a/", PathAction.CreateApplication),
                new PathActionMapItem("/cre_b/", PathAction.CreateBus),

                // Message actions
                new PathActionMapItem("/snd/", PathAction.SendMessage),
                new PathActionMapItem("/rcv/", PathAction.ReceiveMessage),
                new PathActionMapItem("/rcw/", PathAction.ReceiveMessageWait),
                new PathActionMapItem("/srw/", PathAction.SendAndReceiveMessageWait),

                new PathActionMapItem("/wsc/", PathAction.WebSocketConnect),

            };
    }
}