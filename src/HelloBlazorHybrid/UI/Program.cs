using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Samples.HelloBlazorHybrid.Abstractions;
using Samples.HelloBlazorHybrid.Services;
using Stl.Fusion.Client;
using Stl.OS;
using Stl.DependencyInjection;
using Stl.Fusion.Blazor;
using Stl.Fusion.Extensions;

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
        var apiBaseUri = new Uri($"{baseUri}api/");

        // Fusion
        var fusion = services.AddFusion();
        var fusionClient = fusion.AddRestEaseClient(
            (c, o) => {
                o.BaseUri = baseUri;
                o.IsLoggingEnabled = true;
                o.IsMessageLoggingEnabled = false;
            }).ConfigureHttpClientFactory(
            (c, name, o) => {
                var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
                var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
                o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
            });

        // Fusion service clients
        fusionClient.AddReplicaService<ICounterService, ICounterClientDef>();
        fusionClient.AddReplicaService<IWeatherForecastService, IWeatherForecastClientDef>();
        fusionClient.AddReplicaService<IChatService, IChatClientDef>();

        ConfigureSharedServices(services);
    }

    public static void ConfigureSharedServices(IServiceCollection services)
    {
        // Blazorise
        services.AddBlazorise().AddBootstrapProviders().AddFontAwesomeIcons();

        // Other UI-related services
        var fusion = services.AddFusion();
        fusion.AddBlazorUIServices();
        fusion.AddFusionTime();
        fusion.AddBackendStatus<CustomBackendStatus>();
        // We don't care about Sessions in this sample, but IBackendStatus
        // service assumes it's there, so let's register a fake one
        services.AddSingleton(new SessionFactory().CreateSession());

        // Default update delay is set to min.
        services.AddTransient<IUpdateDelayer>(_ => UpdateDelayer.MinDelay);
    }
}
