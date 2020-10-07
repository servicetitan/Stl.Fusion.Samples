using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Samples.Caching.Common;
using Samples.Caching.Server;
using Samples.Caching.Server.Services;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Client;
using Stl.OS;
using static System.Console;

namespace Samples.Caching.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var cts = new CancellationTokenSource();
            CancelKeyPress += (s, ea) => cts.Cancel();
            var cancellationToken = cts.Token;
            var serviceChecker = new ServiceChecker();
            await serviceChecker.WaitForServicesAsync(cancellationToken);

            var localServices = await CreateLocalServiceProviderAsync(cancellationToken);
            var benchmark = new TenantBenchmark(localServices) {
                TimeCheckOperationIndexMask = 7
            };
            await benchmark.InitAsync(cancellationToken);

            benchmark.Services = localServices;
            benchmark.ConcurrencyLevel = HardwareInfo.ProcessorCount - 2;
            benchmark.DumpParameters();
            benchmark.TenantServiceResolver = c => c.GetRequiredService<ITenantService>();
            WriteLine();
            WriteLine("Local services:");
            await benchmark.RunAsync("Fusion's Compute Service [-> EF Core -> SQL Server]", cancellationToken);
            benchmark.TenantServiceResolver = c => c.GetRequiredService<ISqlTenantService>();
            await benchmark.RunAsync("Regular Service [-> EF Core -> SQL Server]", cancellationToken);

            WriteLine();
            WriteLine("Remote services:");
            var remoteServices = await CreateRemoteServiceProviderAsync(cancellationToken);
            benchmark.Services = remoteServices;
            benchmark.TenantServiceResolver = c => c.GetRequiredService<ITenantService>();
            await benchmark.RunAsync("Fusion's Replica Client [-> HTTP+WebSocket -> ASP.NET Core -> Compute Service -> EF Core -> SQL Server]", cancellationToken);
            benchmark.TenantServiceResolver = c => c.GetRequiredService<IRestEaseTenantService>();
            await benchmark.RunAsync("RestEase Client [-> HTTP -> ASP.NET Core -> Compute Service -> EF Core -> SQL Server]", cancellationToken);
            benchmark.TenantServiceResolver = c => c.GetRequiredService<ISqlTenantService>();
            await benchmark.RunAsync("RestEase Client [-> HTTP -> ASP.NET Core -> Regular Service -> EF Core -> SQL Server]", cancellationToken);
        }

        public static Task<IServiceProvider> CreateRemoteServiceProviderAsync(CancellationToken cancellationToken)
        {
            var services = new ServiceCollection();
            var baseUri = new Uri($"http://localhost:5010/");
            var apiBaseUri = new Uri($"{baseUri}api/");

            var fusion = services.AddFusion();
            var fusionClient = fusion.AddRestEaseClient((c, options) => {
                options.BaseUri = baseUri;
                options.MessageLogLevel = LogLevel.Information;
            }).ConfigureHttpClientFactory((c, name, options) => {
                options.HttpClientActions.Add(c => c.BaseAddress = apiBaseUri);
            });
            services.AttributeBased().AddServicesFrom(Assembly.GetExecutingAssembly());
            return Task.FromResult((IServiceProvider) services.BuildServiceProvider());
        }

        public static async Task<IServiceProvider> CreateLocalServiceProviderAsync(CancellationToken cancellationToken)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((ctx, builder) => {
                    // Looks like there is no better way to set _default_ URL
                    builder.Sources.Insert(0, new MemoryConfigurationSource() {
                        InitialData = new Dictionary<string, string>() {
                            {WebHostDefaults.EnvironmentKey, "Production"},
                            {WebHostDefaults.ServerUrlsKey, "http://localhost:0"},
                        }
                    });
                })
                .ConfigureWebHostDefaults(builder => builder
                    .UseDefaultServiceProvider((ctx, options) => {
                        options.ValidateScopes = ctx.HostingEnvironment.IsDevelopment();
                        options.ValidateOnBuild = true;
                    })
                    .UseStartup<Startup>())
                .Build();

            var services = host.Services;
            var dbInitializer = services.GetRequiredService<DbInitializer>();
            await dbInitializer.InitializeAsync(true, cancellationToken);
            return services;
        }
    }
}
