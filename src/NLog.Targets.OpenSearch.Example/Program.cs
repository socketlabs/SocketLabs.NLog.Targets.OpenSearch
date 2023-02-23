using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
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
                using (logger.BeginScope(new[] { new KeyValuePair<string, object>("TransctionId", Guid.NewGuid().ToString()) }))
                {
                    logger.LogInformation("Hello OpenSearch Logging");

                    try
                    {
                        var nestedEx3 = new Exception("this is a deep nested exception 3.");
                        var nestedEx2 = new Exception("this is a deep nested exception 2.", nestedEx3);
                        var nestedEx = new Exception("this is a deep nested exception.", nestedEx2);
                        var innerEx = new ArgumentNullException("Something was null.", nestedEx);
                        var outerEx = new Exception("Ruh roh. Something went wrong.", innerEx);

                        throw outerEx;

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
}
