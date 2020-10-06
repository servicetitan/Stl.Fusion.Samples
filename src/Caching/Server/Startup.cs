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
using Microsoft.OpenApi.Models;
using Samples.Caching.Common;
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
            services.AddLogging(logging => {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
            });

            // DbContext & related services
            services.AddDbContext<AppDbContext>(builder => {
                builder.UseSqlServer(
                    $"Server=127.0.0.1,5020; " +
                    $"Database={ServerSettings.DatabaseName}; " +
                    $"User Id=sa; Password=Fusion.0.to.1;",
                    sqlServer => { });
            });

            // Fusion services
            services.AddSingleton(new Publisher.Options() { Id = CommonSettings.PublisherId });
            var fusion = services.AddFusion();
            var fusionServer = fusion.AddWebSocketServer();
            // This method registers services marked with any of ServiceAttributeBase descendants, including:
            // [Service], [ComputeService], [RestEaseReplicaService], [LiveStateUpdater]
            services.AttributeBased().AddServicesFrom(Assembly.GetExecutingAssembly());

            services.AddRouting();
            services.AddMvc().AddApplicationPart(Assembly.GetExecutingAssembly());

            // Swagger & debug tools
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "Samples.Caching.Server API", Version = "v1"
                });
            });
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
                ReceiveBufferSize = 16_384,
                KeepAliveInterval = TimeSpan.FromSeconds(30),
            });

            // Static + Swagger
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
            });

            // API controllers
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapFusionWebSocketServer();
                endpoints.MapControllers();
            });
        }
    }
}
