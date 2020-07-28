using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cleanic.Framework
{
    public class ProgramBase
    {
        public static IHost BuildHost<T>()
            where T : class
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureLogging((host, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(host.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddAzureWebAppDiagnostics();
                })
                .ConfigureWebHostDefaults(builder => builder.UseStartup<T>());

            return hostBuilder.Build();
        }
    }
}