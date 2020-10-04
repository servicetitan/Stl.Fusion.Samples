using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Samples.Caching.Client;
using Samples.Caching.Server.Services;

namespace Samples.Caching.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((ctx, builder) => {
                    // Looks like there is no better way to set _default_ URL
                    builder.Sources.Insert(0, new MemoryConfigurationSource() {
                        InitialData = new Dictionary<string, string>() {
                            {WebHostDefaults.ServerUrlsKey, "http://localhost:5010"},
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

            // Ensure the DB is re-created
            using (var scope = host.Services.CreateScope()) {
                await using var dbContext = scope.ServiceProvider.RentDbContext<AppDbContext>();
                await dbContext.Database.EnsureDeletedAsync();
                await dbContext.Database.EnsureCreatedAsync();
                await dbContext.Database.ExecuteSqlRawAsync(
                    $"ALTER DATABASE {Settings.DatabaseName} SET ALLOW_SNAPSHOT_ISOLATION ON");
                await dbContext.Database.ExecuteSqlRawAsync(
                    $"ALTER DATABASE {Settings.DatabaseName} SET RECOVERY SIMPLE WITH NO_WAIT");
            }

            await host.RunAsync();
        }
    }
}
