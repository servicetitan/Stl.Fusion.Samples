using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pluralize.NET;
using Samples.Blazor.Abstractions;
using Samples.Blazor.Client;
using Samples.Blazor.UI.Services;
using Stl.Async;
using Stl.CommandR.Interception;
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
            builder.RootComponents.Add<App>("#app");
            var host = builder.Build();

            await host.Services.HostedServices().Start();
            await host.RunAsync();
        }

        public static void ConfigureServices(IServiceCollection services, WebAssemblyHostBuilder builder)
        {
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
            var apiBaseUri = new Uri($"{baseUri}api/");

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

            fusion.AddFusionTime();
            fusionClient.AddReplicaService<ITimeService, ITimeClient>();
            fusionClient.AddReplicaService<IScreenshotService, IScreenshotClient>();
            fusionClient.AddReplicaService<IChatService, IChatClient>();
            fusionClient.AddReplicaService<IComposerService, IComposerClient>();
            fusionClient.AddReplicaService<ISumService, ISumClient>();

            ConfigureSharedServices(services);
        }

        public static void ConfigureSharedServices(IServiceCollection services)
        {
            // Blazorise
            services.AddBlazorise().AddBootstrapProviders().AddFontAwesomeIcons();
            // Default update delayer
            services.AddSingleton<IUpdateDelayer>(_ => new UpdateDelayer(0.1));
            // Other UI-related services
            var fusion = services.AddFusion();
            fusion.AddComputeService<ILocalComposerService, LocalComposerService>();
            var fusionClient = fusion.AddRestEaseClient();
            fusion.AddFusionTime();
            fusionClient.AddReplicaService<ITimeService, ITimeClient>();
            fusionClient.AddReplicaService<IScreenshotService, IScreenshotClient>();
            fusionClient.AddReplicaService<IChatService, IChatClient>();
            fusionClient.AddReplicaService<IComposerService, IComposerClient>();
            fusionClient.AddReplicaService<ISumService, ISumClient>();
        }
    }
}
