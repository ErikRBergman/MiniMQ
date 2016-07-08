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
    using MiniMQ.Model.Core.Message;
    using MiniMQ.Model.Core.MessageHandler;

    public class Application
    {
        private IMessageHandlerContainer messageHandlerContainer;
        private IMessageHandlerProducer messageHandlerProducer;

        private HealthChecker healthChecker = new HealthChecker(TimeSpan.FromSeconds(10));

        public IMessageFactory MessageFactory { get; private set; } = new MessageFactory();

        public Router Router { get; private set; }

        public Application()
        {
        }

        public void Startup(IServiceCollection services)
        {
            this.messageHandlerContainer = new MessageHandlerContainer();

            this.messageHandlerProducer = new MessageHandlerProducer(
                new MessageQueueFactory(
                    this.MessageFactory, 
                    new QueueWebSocketSubscriberFactory(WebSubscriberSettings.Default)),
                new MessageApplicationFactory(
                    this.MessageFactory,
                    new ApplicationWebSocketSubscriberFactory(WebSubscriberSettings.Default)),
                    new MessageBusFactory());

            this.healthChecker.Start();

            this.Router = new Router(this.messageHandlerContainer, this.messageHandlerProducer, new WebSocketConnector(this.healthChecker));

            services.AddSingleton<IMessageHandlerContainer>(this.messageHandlerContainer);
            services.AddSingleton<IMessageHandlerProducer>(this.messageHandlerProducer);
        }
    }
}
