using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Core.Core
{
    public interface IHealthCheckBase
    {
        
    }

    public interface IHealthCheck : IHealthCheckBase
    {
        /// <summary>
        /// The do a health check.
        /// </summary>
        /// <returns>
        ///  Returns false if no more health checks are necessary for this instance
        /// </returns>
        bool DoHealthCheck();
    }

    public interface IHealthCheckAsync : IHealthCheckBase
    {
        Task<bool> DoHealthCheckAsync();
    }
}
