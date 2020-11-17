using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Samples.Caching.Server.Services;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Bridge;
using Stl.Fusion.Server;

namespace Samples.Caching.Server
{
    public class Startup
    {
        private IConfiguration Cfg { get; }
        private IWebHostEnvironment Env { get; }
        private ILogger Log { get; set; } = NullLogger<Startup>.Instance;

        public Startup(IConfiguration cfg, IWebHostEnvironment environment)
        {
            Cfg = cfg;
            Env = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Logging
            services.AddLogging(logging => {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
            });

            // DbContext & related services
            services.AddPooledDbContextFactory<AppDbContext>((c, builder) => {
                var dbSettings = c.GetRequiredService<DbSettings>();
                var connectionString =
                    $"Server={dbSettings.ServerHost},{dbSettings.ServerPort}; " +
                    $"Database={dbSettings.DatabaseName}; " +
                    $"User Id=sa; Password=Fusion.0.to.1; " +
                    $"MultipleActiveResultSets=True; ";
                builder.UseSqlServer(connectionString, sqlServer => { });
            }, 512);

            // Fusion services
            services.AddSingleton(c => {
                var serverSettings = c.GetRequiredService<ServerSettings>();
                return new Publisher.Options() {Id = serverSettings.PublisherId};
            });
            var fusion = services.AddFusion();
            var fusionServer = fusion.AddWebSocketServer();
            // This method registers services marked with any of ServiceAttributeBase descendants, including:
            // [Service], [ComputeService], [RestEaseReplicaService], [LiveStateUpdater]
            services.AttributeBased().AddServicesFrom(Assembly.GetExecutingAssembly());

            services.AddRouting();
            services.AddMvc().AddApplicationPart(Assembly.GetExecutingAssembly());
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> log)
        {
            Log = log;

            if (Env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseWebSockets(new WebSocketOptions() {
                KeepAliveInterval = TimeSpan.FromSeconds(30),
            });

            // Static + Swagger
            app.UseStaticFiles();

            // API controllers
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapFusionWebSocketServer();
                endpoints.MapControllers();
            });
        }
    }
}
