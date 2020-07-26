using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using RestEase;
using Samples.Blazor.Common.Services;
using Samples.Blazor.Server.Services;
using Stl.Fusion;
using Stl.Fusion.Bridge;
using Stl.Fusion.Server;
using Stl.IO;
using Stl.Serialization;

namespace Samples.Blazor.Server
{
    public class Startup
    {
        private IConfiguration Cfg { get; }

        public Startup(IConfiguration cfg) => Cfg = cfg;

        public void ConfigureServices(IServiceCollection services)
        {
            // DbContext & related services
            var appTempDir = PathEx.GetApplicationTempDirectory("", true);
            var dbPath = appTempDir & "Chat.db";
            services
                .AddEntityFrameworkSqlite()
                .AddDbContextPool<ChatDbContext>(builder => {
                    builder.UseSqlite($"Data Source={dbPath}", sqlite => { });
                });
            services.AddSingleton<ChatDbContextPool>();

            // Fusion services
            services.AddSingleton(new Publisher.Options() { Id = Settings.PublisherId });
            services.AddFusionWebSocketServer();
            services.AddComputedService<ITimeService, TimeService>();
            services.AddComputedService<IChatService, ChatService>();
            services.AddComputedService<IComposerService, ServerSideComposerService>();
            services.AddSingleton(c => new RestClient(new Uri("https://uzby.com/api.php"))
                .For<IUzbyClient>());
            services.AddSingleton(c => new RestClient(new Uri("https://api.forismatic.com/api/1.0/"))
                .For<IForismaticClient>());
            services.AddComputedService<IScreenshotService, ScreenshotService>();

            // Web
            services.AddRouting();
            services.AddControllers()
                .AddApplicationPart(Assembly.GetExecutingAssembly());
            services.AddMvc()
                .AddNewtonsoftJson(options => {
                    var settings = options.SerializerSettings;
                    var expected = JsonNetSerializer.DefaultSettings;
                    settings.SerializationBinder = expected.SerializationBinder;
                    settings.TypeNameAssemblyFormatHandling = expected.TypeNameAssemblyFormatHandling;
                    settings.TypeNameHandling = TypeNameHandling.All;
                    settings.NullValueHandling = expected.NullValueHandling;
                });

            // Swagger & debug tools
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "Stl.Sample.Blazor.Server API", Version = "v1"
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseWebSockets(new WebSocketOptions() {
                ReceiveBufferSize = 16_384,
                KeepAliveInterval = TimeSpan.FromSeconds(15),
            });

            // Static + Swagger
            app.UseBlazorFrameworkFiles();
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
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
