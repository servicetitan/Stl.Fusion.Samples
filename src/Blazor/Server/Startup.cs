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
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Samples.Blazor.Abstractions;
using Samples.Blazor.Server.Services;
using Samples.Blazor.UI.Services;
using Stl.DependencyInjection;
using Stl.Fusion.Authentication;
using Stl.Fusion.Blazor;
using Stl.Fusion.Blazor.Authentication;
using Stl.Fusion.EntityFramework;
using Stl.Fusion.Server;
using Stl.IO;
using Stl.RestEase;
using Stl.Rpc;
using Stl.Rpc.Server;

namespace Samples.Blazor.Server;

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
        var appTempDir = FilePath.GetApplicationTempDirectory("", true);
        var dbPath = appTempDir & "App_v011.db";
        services.AddDbContextFactory<AppDbContext>(db => {
            db.UseSqlite($"Data Source={dbPath}");
            if (Env.IsDevelopment())
                db.EnableSensitiveDataLogging();
        });
        services.AddDbContextServices<AppDbContext>(db => {
            db.AddEntityResolver<long, ChatMessage>();
            db.AddOperations(operations => {
                operations.ConfigureOperationLogReader(_ => new() {
                    // We use FileBasedDbOperationLogChangeTracking, so unconditional wake up period
                    // can be arbitrary long - all depends on the reliability of Notifier-Monitor chain.
                    // See what .ToRandom does - most of timeouts in Fusion settings are RandomTimeSpan-s,
                    // but you can provide a normal one too - there is an implicit conversion from it.
                    UnconditionalCheckPeriod = TimeSpan.FromSeconds(Env.IsDevelopment() ? 60 : 5).ToRandom(0.05),
                });
                operations.AddFileBasedOperationLogChangeTracking();
            });
        });

        // Fusion
        var fusion = services.AddFusion(RpcServiceMode.Server, true);
        var fusionServer = fusion.AddWebServer();
        fusion.AddDbAuthService<AppDbContext, long>();
        fusionServer.ConfigureAuthEndpoint(_ => new() {
            DefaultSignInScheme = MicrosoftAccountDefaults.AuthenticationScheme,
            SignInPropertiesBuilder = (_, properties) => {
                properties.IsPersistent = true;
            }
        });
        fusionServer.ConfigureServerAuthHelper(_ => new() {
            NameClaimKeys = Array.Empty<string>(),
        });
        services.AddSingleton(new PresenceReporter.Options() { UpdatePeriod = TimeSpan.FromMinutes(1) });

        // Fusion services
        fusion.AddService<ITimeService, TimeService>();
        fusion.AddService<ISumService, SumService>();
        fusion.AddService<IComposerService, ComposerService>();
        fusion.AddService<IScreenshotService, ScreenshotService>();
        fusion.AddService<IChatService, ChatService>();

        // RestEase clients
        var restEase = services.AddRestEase();
        restEase.AddClient<IForismaticClient>();

        // Data protection
        services.AddScoped(c => c.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
        services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();

        // ASP.NET Core authentication providers
        services.AddAuthentication(options => {
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddCookie(options => {
            options.LoginPath = "/signIn";
            options.LogoutPath = "/signOut";
            if (Env.IsDevelopment())
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            // This controls the expiration time stored in the cookie itself
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
            // And this controls when the browser forgets the cookie
            options.Events.OnSigningIn = ctx => {
                ctx.CookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(28);
                return Task.CompletedTask;
            };
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

        // ASP.NET Core / Blazor services
        services.AddRazorPages();
        services.AddServerSideBlazor(o => o.DetailedErrors = true);

        // Shared UI services
        UI.Program.ConfigureSharedServices(services);
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
            wwwRootPath = Path.GetFullPath(Path.Combine(baseDir, $"../../../../UI/{binCfgPart}/net8.0/wwwroot"));
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

        // Blazor framework + static files
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        // Endpoints
        app.UseRouting();
        app.UseAuthentication();
        app.UseEndpoints(endpoints => {
            endpoints.MapBlazorHub();
            endpoints.MapRpcWebSocketServer();
            endpoints.MapFusionAuth();
            endpoints.MapFusionBlazorMode();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}
