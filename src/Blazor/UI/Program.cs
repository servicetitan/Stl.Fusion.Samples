using System;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samples.Blazor.Abstractions;
using Samples.Blazor.Client;
using Samples.Blazor.UI.Services;
using Stl.Fusion;
using Stl.Fusion.Client;
using Stl.OS;
using Stl.DependencyInjection;
using Stl.Fusion.Blazor;
using Stl.Fusion.Extensions;

namespace Samples.Blazor.UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (!OSInfo.IsWebAssembly)
                throw new ApplicationException("This app runs only in browser.");

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            ConfigureServices(builder.Services, builder);
            var host = builder.Build();
            await host.Services.HostedServices().Start();
            await host.RunAsync();
        }

        public static void ConfigureServices(IServiceCollection services, WebAssemblyHostBuilder builder)
        {
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

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
            var fusionAuth = fusion.AddAuthentication().AddRestEaseClient().AddBlazor();

            // Fusion services
            fusionClient.AddReplicaService<ITimeService, ITimeClientDef>();
            fusionClient.AddReplicaService<IScreenshotService, IScreenshotClientDef>();
            fusionClient.AddReplicaService<IChatService, IChatClientDef>();
            fusionClient.AddReplicaService<IComposerService, IComposerClientDef>();
            fusionClient.AddReplicaService<ISumService, ISumClientDef>();

            ConfigureSharedServices(services);
        }

        public static void ConfigureSharedServices(IServiceCollection services)
        {
            // Blazorise
            services.AddBlazorise().AddBootstrapProviders().AddFontAwesomeIcons();

            // Fusion services
            var fusion = services.AddFusion();
            fusion.AddFusionTime();
            fusion.AddComputeService<ILocalComposerService, LocalComposerService>();

            // Default update delay is 0.1s
            services.AddTransient<IUpdateDelayer>(_ => new UpdateDelayer(0.1));
        }
    }
}
