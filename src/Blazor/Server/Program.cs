using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
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
                .ConfigureHostConfiguration(cfg => {
                    // Looks like there is no better way to set _default_ URL
                    cfg.Sources.Insert(0, new MemoryConfigurationSource() {
                        InitialData = new Dictionary<string, string>() {
                            {"ASPNETCORE_URLS", "http://localhost:5005"},
                        }
                    });
                })
                .ConfigureWebHostDefaults(b => b.UseStartup<Startup>())
                .Build();

            // Ensure the DB is created
            using (var scope = host.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                var chatDbContext = services.GetRequiredService<ChatDbContext>();
                await chatDbContext.Database.EnsureCreatedAsync();
            }

            await host.RunAsync();
        }
    }
}
