using MiniMQ.Core.Message;
using MiniMQ.Core.MessageHandlers.InMemory.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniMq.WebApi
{
    using Microsoft.Extensions.DependencyInjection;

    using MiniMq.WebApi.Routing;

    using MiniMQ.Core.MessageHandler;
    using MiniMQ.Core.MessageHandlers.General;
    using MiniMQ.Core.MessageHandlers.InMemory.Bus;
    using MiniMQ.Core.MessageHandlers.InMemory.Queue;
    using MiniMQ.Model.Core.MessageHandler;

    public class Application
    {
        private IWebSocketSubscriberFactory webSocketSubscriberFactory;
        private IMessageHandlerContainer messageHandlerContainer;
        private IMessageHandlerProducer messageHandlerProducer;

        private HealthChecker healthChecker = new HealthChecker(TimeSpan.FromSeconds(10));

        public Router Router { get; private set; }

        public Application()
        {
        }

        public void Startup(IServiceCollection services)
        {
            this.webSocketSubscriberFactory = new WebSocketSubscriberFactory(WebSubscriberSettings.Default);
            this.messageHandlerContainer = new MessageHandlerContainer();
            this.messageHandlerProducer = new MessageHandlerProducer(
                new MessageQueueFactory(new MessageFactory(), this.webSocketSubscriberFactory),
                new MessageApplicationFactory(
                    new MessageFactory(),
                    this.webSocketSubscriberFactory),
                    new MessageBusFactory());

            this.healthChecker.Start();

            this.Router = new Router(this.messageHandlerContainer, this.messageHandlerProducer, this.healthChecker);

            services.AddSingleton<IMessageHandlerContainer>(this.messageHandlerContainer);
            services.AddSingleton<IMessageHandlerProducer>(this.messageHandlerProducer);
            services.AddSingleton<IWebSocketSubscriberFactory>(this.webSocketSubscriberFactory);

        }
    }
}
