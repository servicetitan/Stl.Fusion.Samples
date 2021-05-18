using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
using Stl.Fusion.Server.Controllers;
using Stl.OS;
using static System.Console;

namespace Samples.Caching.Client
{
    class Program
    {
        public const string ClientSideScope = nameof(ClientSideScope);

        static async Task Main(string[] args)
        {
            using var cts = new CancellationTokenSource();
            CancelKeyPress += (s, ea) => cts.Cancel();
            var cancellationToken = cts.Token;

            var localServices = await CreateLocalServiceProvider(cancellationToken);
            await localServices.GetRequiredService<ServiceChecker>().WaitForServices(cancellationToken);
            await localServices.GetRequiredService<DbInitializer>().Initialize(true, cancellationToken);

            var benchmark = new TenantBenchmark(localServices) {
                TimeCheckOperationIndexMask = 7
            };
            await benchmark.Initialize(cancellationToken);

            benchmark.Services = localServices;
            benchmark.ConcurrencyLevel = HardwareInfo.ProcessorCount - 2;
            benchmark.DumpParameters();
            benchmark.TenantServiceResolver = c => c.GetRequiredService<ITenantService>();
            WriteLine();
            WriteLine("Local services:");
            await benchmark.Run("Fusion's Compute Service [-> EF Core -> SQL Server]", cancellationToken);
            benchmark.TenantServiceResolver = c => c.GetRequiredService<ISqlTenantService>();
            await benchmark.Run("Regular Service [-> EF Core -> SQL Server]", cancellationToken);

            WriteLine();
            WriteLine("Remote services:");
            var remoteServices = await CreateRemoteServiceProvider(cancellationToken);
            benchmark.Services = remoteServices;
            benchmark.TenantServiceResolver = c => c.GetRequiredService<ITenantService>();
            await benchmark.Run("Fusion's Replica Client [-> HTTP+WebSocket -> ASP.NET Core -> Compute Service -> EF Core -> SQL Server]", cancellationToken);
            benchmark.TenantServiceResolver = c => c.GetRequiredService<IRestEaseTenantService>();
            await benchmark.Run("RestEase Client [-> HTTP -> ASP.NET Core -> Compute Service -> EF Core -> SQL Server]", cancellationToken);
            benchmark.TenantServiceResolver = c => c.GetRequiredService<ISqlTenantService>();
            await benchmark.Run("RestEase Client [-> HTTP -> ASP.NET Core -> Regular Service -> EF Core -> SQL Server]", cancellationToken);
        }

        public static Task<IServiceProvider> CreateRemoteServiceProvider(CancellationToken cancellationToken)
        {
            var services = new ServiceCollection();
            var cfg = Host.CreateDefaultBuilder().Build().Services.GetRequiredService<IConfiguration>();
            services.AddSingleton(cfg);

            var fusion = services.AddFusion();
            var fusionClient = fusion.AddRestEaseClient((c, options) => {
                var clientSettings = c.GetRequiredService<ClientSettings>();
                options.BaseUri = clientSettings.BaseUri;
                options.IsLoggingEnabled = false;
            }).ConfigureHttpClientFactory((c, name, options) => {
                var clientSettings = c.GetRequiredService<ClientSettings>();
                options.HttpClientActions.Add(c => c.BaseAddress = clientSettings.ApiBaseUri);
            });
            return Task.FromResult((IServiceProvider) services.BuildServiceProvider());
        }

        public static Task<IServiceProvider> CreateLocalServiceProvider(CancellationToken cancellationToken)
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
                    .ConfigureServices((ctx, services) => {
                    })
                    .UseStartup<Startup>())
                .Build();
            return Task.FromResult(host.Services);
        }
    }
}
