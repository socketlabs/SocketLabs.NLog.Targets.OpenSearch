using NLog.Extensions.Logging;
using NLog.Web;

namespace SocketLabs.NLog.Targets.OpenSearch.AspNetCoreExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseNLog(new NLogAspNetCoreOptions() { RemoveLoggerFactoryFilter = false });
            //builder.Services.AddLogging(options => 
            //    options.AddNLog(new NLogProviderOptions() { RemoveLoggerFactoryFilter = false })
            //    );
            // Add services to the container.

            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
