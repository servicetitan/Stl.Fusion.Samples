using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Samples.HelloBlazorHybrid.Abstractions;
using Samples.HelloBlazorHybrid.Services;
using Stl.Fusion.Blazor;
using Stl.Fusion.Extensions;
using Stl.Fusion.Server;
using Stl.Rpc;
using Stl.Rpc.Server;

namespace Samples.HelloBlazorHybrid.Server;

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
            if (Env.IsDevelopment()) {
                logging.AddFilter("Microsoft", LogLevel.Warning);
                logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
                logging.AddFilter("Stl.Fusion.Operations", LogLevel.Information);
            }
        });

#pragma warning disable ASP0000
        var tmpServices = services.BuildServiceProvider();
#pragma warning restore ASP0000
        Log = tmpServices.GetRequiredService<ILogger<Startup>>();

        // Fusion
        var fusion = services.AddFusion(RpcServiceMode.Server);
        fusion.AddWebServer();
        fusion.AddFusionTime(); // IFusionTime is one of built-in compute services you can use
        services.AddScoped<BlazorModeHelper>();

        // Fusion services
        fusion.AddService<ICounterService, CounterService>();
        fusion.AddService<IWeatherForecastService, WeatherForecastService>();
        fusion.AddService<IChatService, ChatService>();
        fusion.AddService<ChatBotService>();
        // This is just to make sure ChatBotService.StartAsync is called on startup
        services.AddHostedService(c => c.GetRequiredService<ChatBotService>());

        // Shared UI services
        UI.Program.ConfigureSharedServices(services);

        // ASP.NET Core / Blazor services
        services.AddRazorPages();
        services.AddServerSideBlazor(o => o.DetailedErrors = true);
    }

    public void Configure(IApplicationBuilder app, ILogger<Startup> log)
    {
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

        // Blazor + static files
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        // API endpoints
        app.UseRouting();
        app.UseEndpoints(endpoints => {
            endpoints.MapBlazorHub();
            endpoints.MapRpcWebSocketServer();
            endpoints.MapFusionBlazorMode();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}
