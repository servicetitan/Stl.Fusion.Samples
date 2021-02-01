using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using AspNet.Security.OAuth.GitHub;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Models;
using Samples.Blazor.Abstractions;
using Samples.Blazor.Server.Services;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.Blazor;
using Stl.Fusion.Bridge;
using Stl.Fusion.Client;
using Stl.Fusion.EntityFramework;
using Stl.Fusion.Server;
using Stl.IO;

namespace Samples.Blazor.Server
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
#pragma warning disable ASP0000
            var serverSettings = services
                .UseAttributeScanner(s => s.AddService<ServerSettings>())
                .BuildServiceProvider()
                .GetRequiredService<ServerSettings>();
#pragma warning restore ASP0000

            // Logging
            services.AddLogging(logging => {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
                if (Env.IsDevelopment())
                    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
            });

            // DbContext & related services
            var appTempDir = PathEx.GetApplicationTempDirectory("", true);
            var dbPath = appTempDir & "App_v095.db";
            services.AddDbContextFactory<AppDbContext>(b => {
                b.UseSqlite($"Data Source={dbPath}", sqlite => { });
                if (Env.IsDevelopment())
                    b.EnableSensitiveDataLogging();
            });
            services.AddDbContextServices<AppDbContext>(b => {
                // This is the best way to add DbContext-related services from Stl.Fusion.EntityFramework
                b.AddDbEntityResolver<long, ChatMessage>();
                b.AddDbOperations((_, o) => {
                    // We use FileBasedDbOperationLogChangeMonitor, so unconditional wake up period
                    // can be arbitrary long - all depends on the reliability of Notifier-Monitor chain.
                    o.UnconditionalWakeUpPeriod = TimeSpan.FromSeconds(Env.IsDevelopment() ? 60 : 5);
                });
                var operationLogChangeAlertPath = dbPath + "_changed";
                b.AddFileBasedDbOperationLogChangeNotifier(operationLogChangeAlertPath);
                b.AddFileBasedDbOperationLogChangeMonitor(operationLogChangeAlertPath);
                b.AddDbAuthentication();
            });

            // Fusion services
            services.AddSingleton(new Publisher.Options() { Id = serverSettings.PublisherId });
            services.AddSingleton(new PresenceService.Options() { UpdatePeriod = TimeSpan.FromMinutes(1) });
            var fusion = services.AddFusion();
            var fusionServer = fusion.AddWebServer();
            var fusionClient = fusion.AddRestEaseClient();
            var fusionAuth = fusion.AddAuthentication().AddServer();
            // This method registers services marked with any of ServiceAttributeBase descendants, including:
            // [Service], [ComputeService], [RestEaseReplicaService], [LiveStateUpdater]
            services.UseAttributeScanner().AddServicesFrom(Assembly.GetExecutingAssembly());
            // Registering shared services from the client
            UI.Program.ConfigureSharedServices(services);

            services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(options => {
                options.LoginPath = "/signIn";
                options.LogoutPath = "/signOut";
            }).AddMicrosoftAccount(options => {
                options.ClientId = serverSettings.MicrosoftAccountClientId;
                options.ClientSecret = serverSettings.MicrosoftAccountClientSecret;
                // That's for personal account authentication flow
                options.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                options.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            }).AddGitHub(options => {
                options.Scope.Add("read:user");
                options.Scope.Add("user:email");
                options.ClientId = serverSettings.GitHubClientId;
                options.ClientSecret = serverSettings.GitHubClientSecret;
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            });

            // Web
            services.AddRouting();
            services.AddMvc().AddApplicationPart(Assembly.GetExecutingAssembly());
            services.AddServerSideBlazor(o => o.DetailedErrors = true);
            fusionAuth.AddBlazor(o => {}); // Must follow services.AddServerSideBlazor()!

            // Swagger & debug tools
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "Samples.Blazor.Server API", Version = "v1"
                });
            });
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> log)
        {
            Log = log;

            // This server serves static content from Blazor Client,
            // and since we don't copy it to local wwwroot,
            // we need to find Client's wwwroot in bin/(Debug/Release) folder
            // and set it as this server's content root.
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var binCfgPart = Regex.Match(baseDir, @"[\\/]bin[\\/]\w+[\\/]").Value;
            var wwwRootPath = Path.Combine(baseDir, "wwwroot");
            if (!Directory.Exists(Path.Combine(wwwRootPath, "_framework")))
                // This is a regular build, not a build produced w/ "publish",
                // so we remap wwwroot to the client's wwwroot folder
                wwwRootPath = Path.GetFullPath(Path.Combine(baseDir, $"../../../../UI/{binCfgPart}/net5.0/wwwroot"));
            Env.WebRootPath = wwwRootPath;
            Env.WebRootFileProvider = new PhysicalFileProvider(Env.WebRootPath);
            StaticWebAssetsLoader.UseStaticWebAssets(Env, Cfg);

            if (Env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            app.UseWebSockets(new WebSocketOptions() {
                KeepAliveInterval = TimeSpan.FromSeconds(30),
            });
            app.UseFusionSession();

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
            app.ApplicationServices.UseBootstrapProviders().UseFontAwesomeIcons(); // Blazorise
            app.UseEndpoints(endpoints => {
                endpoints.MapBlazorHub();
                endpoints.MapFusionWebSocketServer();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
