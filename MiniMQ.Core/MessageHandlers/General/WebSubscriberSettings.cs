namespace MiniMQ.Core.MessageHandlers.InMemory.Queue
{
    using System;

    public struct WebSubscriberSettings
    {
        public WebSubscriberSettings(TimeSpan connectionStatusCheckInterval)
        {
            this.ConnectionStatusCheckInterval = connectionStatusCheckInterval;
        }

        public TimeSpan ConnectionStatusCheckInterval { get; set; }

        public static WebSubscriberSettings Default => new WebSubscriberSettings(TimeSpan.FromSeconds(10));
    }
}