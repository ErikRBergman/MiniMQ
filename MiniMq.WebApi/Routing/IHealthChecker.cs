namespace MiniMq.WebApi.Routing
{
    using MiniMQ.Core.Core;

    /// <summary>
    /// The HealthChecker interface.
    /// </summary>
    public interface IHealthChecker
    {
        void Add(IHealthCheckBase healthCheck);
    }
}