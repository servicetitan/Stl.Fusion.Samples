using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Samples.Blazor.Common.Services;
using Samples.Blazor.Server.Services;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Bridge;
using Stl.Fusion.Client;
using Stl.Fusion.Server;
using Stl.IO;
using Stl.Reflection;
using Stl.Serialization;

namespace Samples.Blazor.Server
{
    public class Startup
    {
        private IConfiguration Cfg { get; }
        private IWebHostEnvironment Env { get; }

        public Startup(IConfiguration cfg, IWebHostEnvironment environment)
        {
            Cfg = cfg;
            Env = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // DbContext & related services
            var appTempDir = PathEx.GetApplicationTempDirectory("", true);
            var dbPath = appTempDir & "App.db";
            services.AddDbContextPool<AppDbContext>(builder => {
                builder.UseSqlite($"Data Source={dbPath}", sqlite => { });
            });

            // Fusion services
            services.AddSingleton(new Publisher.Options() { Id = Settings.PublisherId });
            var fusion = services.AddFusion();
            var fusionServer = fusion.AddWebSocketServer();
            var fusionClient = fusion.AddRestEaseClient();
            // This method registers services marked with any of ServiceAttributeBase descendants, including:
            // [Service], [ComputeService], [RestEaseReplicaService], [LiveStateUpdater]
            services.AttributeBased().AddServicesFrom(Assembly.GetExecutingAssembly());
            // Registering shared services from the client
            Client.Program.ConfigureSharedServices(services);

            // Session
            services.AddDistributedMemoryCache();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(1);
                options.Cookie.HttpOnly = true;
            });

            // Authentication
            services.AddAuthentication(options => {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options => {
                    options.LoginPath = "/signin";
                    options.LogoutPath = "/signout";
                })
                .AddGitHub(options => {
                    options.ClientId = Cfg["Authentication:GitHub:ClientId"] ?? "<None>";
                    options.ClientSecret = Cfg["Authentication:GitHub:ClientSecret"] ?? "<None>";
                });

            // Web
            services.AddRouting();
            services.AddMvc()
                .AddApplicationPart(Assembly.GetExecutingAssembly())
                .AddNewtonsoftJson(options => MemberwiseCopier.CopyMembers(
                    JsonNetSerializer.DefaultSettings,
                    options.SerializerSettings));
            services.AddServerSideBlazor();

            // Swagger & debug tools
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "Stl.Sample.Blazor.Server API", Version = "v1"
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            // This server serves static content from Blazor Client,
            // and since we don't copy it to local wwwroot,
            // we need to find Client's wwwroot in bin/(Debug/Release) folder
            // and set it as this server's content root.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var binCfgPart = Regex.Match(baseDir, @"[\\/]bin[\\/]\w+[\\/]").Value;
            Env.WebRootPath = Path.GetFullPath(Path.Combine(baseDir,
                $"../../../../Client/{binCfgPart}/netstandard2.1/")) + "wwwroot";
            Env.WebRootFileProvider = new PhysicalFileProvider(Env.WebRootPath);

            if (Env.IsDevelopment()) {
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
            app.UseSession();

            // Static + Swagger
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
            });

            // API controllers
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapBlazorHub();
                endpoints.MapFusionWebSocketServer();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
