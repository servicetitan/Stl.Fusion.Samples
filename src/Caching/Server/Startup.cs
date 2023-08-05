using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Samples.Caching.Common;
using Samples.Caching.Server.Services;
using Stl.DependencyInjection;
using Stl.Fusion.Server;
using Stl.Rpc;
using Stl.Rpc.Server;

namespace Samples.Caching.Server;

public class Startup
{
    private IConfiguration Cfg { get; }
    private IWebHostEnvironment Env { get; }
    private ServerSettings ServerSettings { get; set; } = null!;
    private DbSettings DbSettings { get; set; } = null!;
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

        // Creating Log, HostSettings, and DbSettings as early as possible
        services.AddSettings<ServerSettings>("Server");
        services.AddSettings<DbSettings>("DB");
#pragma warning disable ASP0000
        var tmpServices = services.BuildServiceProvider();
#pragma warning restore ASP0000
        Log = tmpServices.GetRequiredService<ILogger<Startup>>();
        ServerSettings = tmpServices.GetRequiredService<ServerSettings>();
        DbSettings = tmpServices.GetRequiredService<DbSettings>();

        // DbContext & related services
        services.AddPooledDbContextFactory<AppDbContext>((c, builder) => {
            var connectionString =
                $"Server={DbSettings.ServerHost},{DbSettings.ServerPort}; " +
                $"Database={DbSettings.DatabaseName}; " +
                $"User Id=sa; Password=SqlServer1; " +
                $"MultipleActiveResultSets=True; ";
            builder.UseSqlServer(connectionString, sqlServer => { });
        }, 512);

        // Fusion
        var fusion = services.AddFusion(RpcServiceMode.Server, true);
        var fusionServer = fusion.AddWebServer();

        // Fusion services
        fusion.AddService<ITenantService, TenantService>();

        // Other services
        services.AddSingleton<DbInitializer>();
        services.AddSingleton<ISqlTenantService, TenantService>(); // Non-Fusion version of ITenantService

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

        // Endpoints
        app.UseRouting();
        app.UseEndpoints(endpoints => {
            endpoints.MapRpcWebSocketServer();
            endpoints.MapControllers();
        });
    }
}
