using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;

namespace Azure.Servers.NetFrameworkFunctions
{
    internal class Program
    {
        static IConfiguration Configuration;
        static void Main(string[] args)
        {
            FunctionsDebugger.Enable();

            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices((hostBuilderContext, services) => {
                    Configuration = hostBuilderContext.Configuration;
                    Debug.WriteLine("config: " + Configuration["CacheConnection"]);
                    services.AddSingleton(async x => await RedisConnection.InitializeAsync(connectionString: Configuration["CacheConnection"].ToString()));
                })

                .Build();

            host.Run();
        }
    }
}
