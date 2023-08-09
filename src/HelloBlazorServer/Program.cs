using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration.Memory;
using Samples.HelloBlazorServer;

var host = Host.CreateDefaultBuilder()
    .ConfigureHostConfiguration(cfg => {
        // Looks like there is no better way to set _default_ URL
        cfg.Sources.Insert(0, new MemoryConfigurationSource() {
            InitialData = new Dictionary<string, string?>() {
                {WebHostDefaults.ServerUrlsKey, "http://localhost:5005"},
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
