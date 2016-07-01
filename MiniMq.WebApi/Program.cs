using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

namespace MiniMq.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string hostUrl = null;

            if (args.Length > 0)
            {
                hostUrl = args[0];
            }

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>();

            if (hostUrl != null)
            {
                host.UseUrls(hostUrl);
            }
            
            var hostBuilder = host.Build();
            hostBuilder.Run();
        }
    }
}
