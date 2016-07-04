namespace MiniMQ.Core
{
    using System.Threading.Tasks;
    using System.Web;

    using MessageHandler;

    using MiniMQ.Core.Message;
    using MiniMQ.Routing;

    public class RequestHandler : HttpTaskAsyncHandler
    {
        public override bool IsReusable => true;

        private static readonly IMessageHandlerContainer MessageHandlerContainer = new MessageHandlerContainer();

        private static readonly IMessageHandlerFactory MessageHandlerFactory = new MessageHandlerFactory(new MessageFactory(), new PassThroughMessageFactory());

        private static readonly Router Router = new Router(MessageHandlerContainer, MessageHandlerFactory);

        public override Task ProcessRequestAsync(HttpContext context)
        {
            var path = context.Request.Path;

            var routeTask = Router.RouteCall(context, path);

            if (routeTask == Router.RouteFailedTask)
            {
                context.Response.Write("Action not recognized: " + context.Request.Path);
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
            }

            return routeTask;
        }

    }
}