using APIsault.Azure.Servers.Functions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;

IConfiguration? Configuration;
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostBuilderContext,services) => {
        Configuration = hostBuilderContext.Configuration;
        Debug.WriteLine("config: " + Configuration["CacheConnection"]);
        services.AddSingleton(async x => await RedisConnection.InitializeAsync(connectionString: Configuration["CacheConnection"].ToString()));
      })

    .Build();
    host.Run();