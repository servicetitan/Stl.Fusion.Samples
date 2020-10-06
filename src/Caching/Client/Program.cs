using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Samples.Caching.Common;
using Samples.Caching.Server;
using Samples.Caching.Server.Services;
using Stl;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Client;

namespace Samples.Caching.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await using var serverRunner = new ServerRunner();
            // ReSharper disable once AccessToDisposedClosure
            Console.CancelKeyPress += (s, ea) => serverRunner.Dispose();
            serverRunner.RunAsync().Ignore();
            await serverRunner.ReadyTask;

            var localServices = await CreateLocalServiceProviderAsync();

            // Benchmarks
            var benchmark = new TenantBenchmark(localServices) {
                TimeCheckOperationIndexMask = 7
            };
            await benchmark.InitializeAsync();

            benchmark.Services = localServices;
            benchmark.TenantServiceResolver = c => c.GetRequiredService<ITenantService>();
            await benchmark.RunAsync("Local Service (Fusion over EF)");
            benchmark.TenantServiceResolver = c => c.GetRequiredService<ISqlTenantService>();
            await benchmark.RunAsync("Local Service (EF)");

            var remoteServices = await CreateRemoteServiceProviderAsync();
            benchmark.Services = remoteServices;
            benchmark.TenantServiceResolver = c => c.GetRequiredService<ITenantService>();
            await benchmark.RunAsync("Remote Service (Fusion over EF)");
            benchmark.TenantServiceResolver = c => c.GetRequiredService<ISqlTenantService>();
            await benchmark.RunAsync("Remote Service (EF)");
        }

        public static Task<IServiceProvider> CreateRemoteServiceProviderAsync()
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

        public static async Task<IServiceProvider> CreateLocalServiceProviderAsync()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((ctx, builder) => {
                    // Looks like there is no better way to set _default_ URL
                    builder.Sources.Insert(0, new MemoryConfigurationSource() {
                        InitialData = new Dictionary<string, string>() {
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
            await dbInitializer.InitializeAsync(true);
            return services;
        }
    }
}
