using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Hosting;

namespace Samples.Caching.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((ctx, builder) => {
                // Looks like there is no better way to set _default_ URL
                builder.Sources.Insert(0, new MemoryConfigurationSource() {
                    InitialData = new Dictionary<string, string>() {
                        {WebHostDefaults.EnvironmentKey, "Production"},
                        {WebHostDefaults.ServerUrlsKey, "http://localhost:5010"},
                    }
                });
            })
            .ConfigureWebHostDefaults(webHost => webHost
                .UseDefaultServiceProvider((ctx, options) => {
                    options.ValidateScopes = ctx.HostingEnvironment.IsDevelopment();
                    options.ValidateOnBuild = true;
                })
                .UseStartup<Startup>())
            .Build();
        await host.RunAsync();
    }
}
