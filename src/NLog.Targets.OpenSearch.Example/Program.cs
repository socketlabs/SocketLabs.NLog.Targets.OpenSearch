using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Threading;

namespace NLog.Targets.OpenSearch.Example
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            services.AddLogging(opt =>
            {
                opt.AddNLog();
                opt.AddConsole();
            });

            var sp = services.BuildServiceProvider();
            var logger = sp.GetRequiredService<ILogger<Program>>();

            while (true)
            {
                logger.LogInformation("Hello OpenSearch Logging");

                try
                {
                    throw new Exception("Ruh roh. Something went wrong.", new ArgumentNullException("Something was null."));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "There was problem.");

                }

                Thread.Sleep(2000);
            }
        }
    }
}
