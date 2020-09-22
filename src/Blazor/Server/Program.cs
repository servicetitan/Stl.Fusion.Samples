using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Samples.Blazor.Server.Services;

namespace Samples.Blazor.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((ctx, builder) => {
                    builder.AddUserSecrets<Program>();
                    // Looks like there is no better way to set _default_ URL
                    builder.Sources.Insert(0, new MemoryConfigurationSource() {
                        InitialData = new Dictionary<string, string>() {
                            {WebHostDefaults.ServerUrlsKey, "http://localhost:5005"},
                        }
                    });
                })
                .ConfigureWebHostDefaults(builder => builder
                    .UseDefaultServiceProvider((ctx, options) => {
                        options.ValidateScopes = ctx.HostingEnvironment.IsDevelopment();
                        options.ValidateOnBuild = true;
                    })
                    .UseStartup<Startup>())
                .Build();

            // Ensure the DB is created
            using (var scope = host.Services.CreateScope()) {
                await using var dbContext = scope.ServiceProvider.RentDbContext();
                await dbContext.Database.EnsureCreatedAsync();
            }

            await host.RunAsync();
        }
    }
}
