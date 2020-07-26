using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RestEase;
using Stl.Fusion;
using Stl.Fusion.Bridge;
using Stl.Fusion.Server;
using Stl.IO;
using Samples.Blazor.Common.Services;
using Samples.Blazor.Server.Services;
using System.Text.RegularExpressions;

namespace Samples.Blazor.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // This server serves static content from Blazor Client,
            // and since we don't copy it to local wwwroot,
            // we need to find Client's wwwroot in bin/(Debug/Release) folder
            // and set it as this server's content root.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var binCfgPart = Regex.Match(baseDir, @"[\\/]bin[\\/]\w+[\\/]").Value;
            var wwwRoot = Path.GetFullPath(Path.Combine(baseDir,
                $"../../../../Client/{binCfgPart}/netstandard2.1/")) + "wwwroot";

            var host = Host.CreateDefaultBuilder()
                .UseContentRoot(wwwRoot)
                .ConfigureWebHostDefaults(builder => {
                    builder.UseUrls("http://localhost:5005");
                    builder.UseWebRoot(wwwRoot);
                    builder.UseStartup<Startup>();
                })
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
