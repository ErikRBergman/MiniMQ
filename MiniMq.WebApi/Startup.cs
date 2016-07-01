using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MiniMq.WebApi
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;

    using MiniMq.WebApi.Routing;

    using MiniMQ.Core.Message;
    using MiniMQ.Core.MessageHandler;

    public class Startup
    {
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

            app.Run(this.Handler);
        }

        private static readonly IMessageHandlerContainer MessageHandlerContainer = new MessageHandlerContainer();

        private static readonly IMessageHandlerFactory MessageHandlerFactory = new MessageHandlerFactory(new MessageFactory(), new MessageFactory());

        private static readonly Router Router = new Router(MessageHandlerContainer, MessageHandlerFactory);

        private Task Handler(HttpContext context)
        {
            return Router.RouteCall(context, context.Request.Path);
        }
    }
}
