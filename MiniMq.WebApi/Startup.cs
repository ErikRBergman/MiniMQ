namespace MiniMq.WebApi
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;

    using Routing;

    using MiniMQ.Core.Message;
    using MiniMQ.Core.MessageHandler;
    using MiniMQ.Core.MessageHandlers.InMemory.Application;
    using MiniMQ.Core.MessageHandlers.InMemory.Bus;
    using MiniMQ.Core.MessageHandlers.InMemory.Queue;
    using MiniMQ.Core.Routing;
    using MiniMQ.Model.Core.MessageHandler;

    public class Startup
    {
        private static readonly IMessageHandlerContainer MessageHandlerContainer = new MessageHandlerContainer();
        private static readonly IMessageHandlerProducer MessageHandlerProducer = new MessageHandlerProducer(new MessageQueueFactory(new MessageFactory()), new MessageApplicationFactory(new MessageFactory()), new MessageBusFactory());
        private static readonly Router Router = new Router(MessageHandlerContainer, MessageHandlerProducer);

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            // services.AddMvc();
            // services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();
            }

            app.UseWebSockets();

            app.Run(this.Handler);
        }


        private Task Handler(HttpContext context)
        {
            var routerResult = Router.RouteCall(context, context.Request.Path);

            var routingResult = routerResult as Task<Router.RouteResult>;
            if (routingResult != null)
            {
                if (ReferenceEquals(routerResult, Router.RouteFailedTask))
                {
                    context.Response.StatusCode = 404;
                    return context.Response.WriteAsync("MiniMQ - Command not found! Try: " + string.Join(", ", PathActionMap.Items.Select(pami => pami.Path)));
                }

                return routingResult.ContinueWith(async rr =>
                    {
                        var result = rr.Result;

                        if (result.Failed)
                        {
                            context.Response.StatusCode = 500;
                            await context.Response.WriteAsync(result.Description);
                        }
                    });
            }

            return routerResult;

        }
    }
}
