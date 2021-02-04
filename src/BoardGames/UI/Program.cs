using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pluralize.NET;
using Samples.BoardGames.Abstractions;
using Stl.Fusion;
using Stl.Fusion.Client;
using Stl.OS;
using Stl.DependencyInjection;
using Stl.Extensibility;
using Stl.Fusion.Blazor;
using Stl.Serialization;

namespace Samples.BoardGames.UI
{
    public class Program
    {
        public const string ClientSideScope = nameof(ClientSideScope);

        public static Task Main(string[] args)
        {
            if (OSInfo.Kind != OSKind.WebAssembly)
                throw new ApplicationException("This app runs only in browser.");

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            ConfigureServices(builder.Services, builder);
            builder.RootComponents.Add<App>("#app");
            var host = builder.Build();

            host.Services.UseBootstrapProviders().UseFontAwesomeIcons(); // Blazorise
            var runTask = host.RunAsync();
            Task.Run(async () => {
                // We "manually" start IHostedServices here, because Blazor host doesn't do this.
                var hostedServices = host.Services.GetRequiredService<IEnumerable<IHostedService>>();
                foreach (var hostedService in hostedServices)
                    await hostedService.StartAsync(default);
            });
            return runTask;
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
                    o.MessageLogLevel = LogLevel.Information;
                }).ConfigureHttpClientFactory(
                (c, name, o) => {
                    var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
                    var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
                    o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
                });
            var fusionAuth = fusion.AddAuthentication().AddRestEaseClient().AddBlazor();

            // This method registers services marked with any of ServiceAttributeBase descendants, including:
            // [Service], [ComputeService], [RestEaseReplicaService], [LiveStateUpdater]
            services.UseAttributeScanner(ClientSideScope).AddServicesFrom(Assembly.GetExecutingAssembly());
            ConfigureSharedServices(services);
        }

        public static void ConfigureSharedServices(IServiceCollection services)
        {
            // Game engines
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IGameEngine, GomokuEngine>());
            services.AddSingleton(c =>
                c.GetRequiredService<IEnumerable<IGameEngine>>().ToImmutableDictionary(e => e.Id));

            // UI services: Blazorise, Pluralizer, etc.
            services.AddSingleton<IPluralize, Pluralizer>();
            services.AddBlazorise(options => {
                    options.DelayTextOnKeyPress = true;
                    options.DelayTextOnKeyPressInterval = 300;
                })
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();

            // Fusion: default delay for update delayers
            services.AddSingleton(c => new UpdateDelayer.Options() {
                Delay = TimeSpan.FromSeconds(0.1),
            });

            // Other UI services
            services.AddSingleton<IMatchingTypeFinder>(new MatchingTypeFinder(typeof(Program).Assembly));

            // This method registers services marked with any of ServiceAttributeBase descendants, including:
            // [Service], [ComputeService], [RestEaseReplicaService], [LiveStateUpdater]
            services.UseAttributeScanner().AddServicesFrom(Assembly.GetExecutingAssembly());
        }
    }
}
