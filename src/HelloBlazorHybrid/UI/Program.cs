using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Samples.HelloBlazorHybrid.Abstractions;
using Samples.HelloBlazorHybrid.Services;
using Stl.Fusion.Client;
using Stl.OS;
using Stl.DependencyInjection;
using Stl.Fusion.Blazor;
using Stl.Fusion.Extensions;
using Stl.Fusion.UI;

namespace Samples.HelloBlazorHybrid.UI;

public class Program
{
    public static Task Main(string[] args)
    {
        if (OSInfo.Kind != OSKind.WebAssembly)
            throw new ApplicationException("This app runs only in browser.");

        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        ConfigureServices(builder.Services, builder);
        var host = builder.Build();

        host.Services.HostedServices().Start();
        return host.RunAsync();
    }

    public static void ConfigureServices(IServiceCollection services, WebAssemblyHostBuilder builder)
    {
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        var baseUri = new Uri(builder.HostEnvironment.BaseAddress);

        // Fusion
        var fusion = services.AddFusion();
        fusion.Rpc.AddWebSocketClient(baseUri);

        // Fusion service clients
        fusion.AddClient<ICounterService>();
        fusion.AddClient<IWeatherForecastService>();
        fusion.AddClient<IChatService>();

        ConfigureSharedServices(services);
    }

    public static void ConfigureSharedServices(IServiceCollection services)
    {
        // Blazorise
        services.AddBlazorise().AddBootstrapProviders().AddFontAwesomeIcons();

        // Other UI-related services
        var fusion = services.AddFusion();
        fusion.AddBlazor();
        fusion.AddFusionTime();

        // Default update delay is set to 0.1s
        services.AddTransient<IUpdateDelayer>(c => new UpdateDelayer(c.UIActionTracker(), 0.1));
    }
}
