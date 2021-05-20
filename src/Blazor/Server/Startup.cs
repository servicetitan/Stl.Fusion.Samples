using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
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
using Samples.Blazor.UI.Services;
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
        private ServerSettings ServerSettings { get; set; } = null!;
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
                if (Env.IsDevelopment()) {
                    logging.AddFilter("Microsoft", LogLevel.Warning);
                    logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
                    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
                    logging.AddFilter("Stl.Fusion.Operations", LogLevel.Information);
                }
            });

            // Creating Log and ServerSettings as early as possible
            services.AddSettings<ServerSettings>("Server");
#pragma warning disable ASP0000
            var tmpServices = services.BuildServiceProvider();
#pragma warning restore ASP0000
            Log = tmpServices.GetRequiredService<ILogger<Startup>>();
            ServerSettings = tmpServices.GetRequiredService<ServerSettings>();

            // DbContext & related services
            var appTempDir = PathEx.GetApplicationTempDirectory("", true);
            var dbPath = appTempDir & "App_v011.db";
            services.AddDbContextFactory<AppDbContext>(b => {
                b.UseSqlite($"Data Source={dbPath}");
                if (Env.IsDevelopment())
                    b.EnableSensitiveDataLogging();
            });
            services.AddDbContextServices<AppDbContext>(b => {
                b.AddDbEntityResolver<long, ChatMessage>();
                b.AddDbOperations((_, o) => {
                    // We use FileBasedDbOperationLogChangeMonitor, so unconditional wake up period
                    // can be arbitrary long - all depends on the reliability of Notifier-Monitor chain.
                    o.UnconditionalWakeUpPeriod = TimeSpan.FromSeconds(Env.IsDevelopment() ? 60 : 5);
                });
                b.AddFileBasedDbOperationLogChangeTracking(dbPath + "_changed");
                b.AddDbAuthentication();
            });

            // Fusion
            var fusion = services.AddFusion();
            var fusionClient = fusion.AddRestEaseClient();
            var fusionServer = fusion.AddWebServer();
            var fusionAuth = fusion.AddAuthentication().AddServer(
                signInControllerOptionsBuilder: (_, options) => {
                    options.DefaultScheme = MicrosoftAccountDefaults.AuthenticationScheme;
                });
            services.AddSingleton(new Publisher.Options() { Id = ServerSettings.PublisherId });
            services.AddSingleton(new PresenceService.Options() { UpdatePeriod = TimeSpan.FromMinutes(1) });

            // Fusion services
            fusionClient.AddClientService<IForismaticClient>();
            fusion.AddComputeService<ITimeService, TimeService>();
            fusion.AddComputeService<ISumService, SumService>();
            fusion.AddComputeService<IComposerService, ComposerService>();
            fusion.AddComputeService<IScreenshotService, ScreenshotService>();
            fusion.AddComputeService<IChatService, ChatService>();

            // Shared UI services
            UI.Program.ConfigureSharedServices(services);

            // Data protection
            services.AddScoped(c => c.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
            services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();

            // ASP.NET Core authentication providers
            services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(options => {
                options.LoginPath = "/signIn";
                options.LogoutPath = "/signOut";
                if (Env.IsDevelopment())
                    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            }).AddMicrosoftAccount(options => {
                options.ClientId = ServerSettings.MicrosoftAccountClientId;
                options.ClientSecret = ServerSettings.MicrosoftAccountClientSecret;
                // That's for personal account authentication flow
                options.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                options.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            }).AddGitHub(options => {
                options.Scope.Add("read:user");
                options.Scope.Add("user:email");
                options.ClientId = ServerSettings.GitHubClientId;
                options.ClientSecret = ServerSettings.GitHubClientSecret;
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            });

            // Web
            services.Configure<ForwardedHeadersOptions>(options => {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
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
            if (ServerSettings.AssumeHttps) {
                Log.LogInformation("AssumeHttps on");
                app.Use((context, next) => {
                    context.Request.Scheme = "https";
                    return next();
                });
            }

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
            app.UseForwardedHeaders(new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });

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
            app.UseEndpoints(endpoints => {
                endpoints.MapBlazorHub();
                endpoints.MapFusionWebSocketServer();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
