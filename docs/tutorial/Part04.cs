using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using RestEase;
using Stl.Fusion;
using Stl.Fusion.Bridge;
using Stl.Fusion.Client;
using Stl.Fusion.Client.RestEase;
using Stl.Fusion.Server;
using Stl.Reflection;
using Stl.Serialization;
using static System.Console;

namespace Tutorial
{
    using Part04_Classes;

    namespace Part04_Classes
    {
        #region Part04_CommonServices
        // Ideally, we want Replica Service to be exactly the same as corresponding
        // Compute Service. A good way to enforce this is to expose an interface
        // that should be implemented by Compute Service + tell Fusion to "expose"
        // the client via the same interface.
        public interface ICounterService
        {
            [ComputeMethod]
            Task<int> GetAsync(string key, CancellationToken cancellationToken = default);
            Task IncrementAsync(string key, CancellationToken cancellationToken = default);
            Task SetOffsetAsync(int offset, CancellationToken cancellationToken = default);
        }
        #endregion

        #region Part04_HostServices
        public class CounterService : ICounterService
        {
            private readonly ConcurrentDictionary<string, int> _counters = new ConcurrentDictionary<string, int>();
            private readonly IMutableState<int> _offset;

            public CounterService(IStateFactory stateFactory)
                => _offset = stateFactory.NewMutable<int>();

            [ComputeMethod] // Optional: this attribute is inherited from interface
            public virtual async Task<int> GetAsync(string key, CancellationToken cancellationToken = default)
            {
                WriteLine($"{nameof(GetAsync)}({key})");
                var offset = await _offset.UseAsync(cancellationToken);
                return offset + (_counters.TryGetValue(key, out var value) ? value : 0);
            }

            public Task IncrementAsync(string key, CancellationToken cancellationToken = default)
            {
                WriteLine($"{nameof(IncrementAsync)}({key})");
                _counters.AddOrUpdate(key, k => 1, (k, v) => v + 1);
                Computed.Invalidate(() => GetAsync(key, default));
                return Task.CompletedTask;
            }

            public Task SetOffsetAsync(int offset, CancellationToken cancellationToken = default)
            {
                WriteLine($"{nameof(SetOffsetAsync)}({offset})");
                _offset.Value = offset;
                return Task.CompletedTask;
            }
        }

        // We need Web API controller to publish the service
        [Route("api/[controller]")]
        [ApiController]
        public class CounterController : FusionController
        {
            private ICounterService Counters { get; }

            public CounterController(IPublisher publisher, ICounterService counterService)
                : base(publisher)
                => Counters = counterService;

            [HttpGet("get")]
            public async Task<int> GetAsync(string key)
            {
                key ??= ""; // Empty value is bound to null value by default
                WriteLine($"{GetType().Name}.{nameof(GetAsync)}({key})");
                // PublishAsync adds Fusion headers enabling the client to create Replica for this response
                return await PublishAsync(ct => Counters.GetAsync(key, ct));
            }

            [HttpPost("inc")]
            public Task IncrementAsync(string key)
            {
                key ??= ""; // Empty value is bound to null value by default
                WriteLine($"{GetType().Name}.{nameof(IncrementAsync)}({key})");
                return Counters.IncrementAsync(key, HttpContext.RequestAborted);
            }

            [HttpPost("setOffset")]
            public Task SetOffsetAsync(int offset)
            {
                WriteLine($"{GetType().Name}.{nameof(SetOffsetAsync)}({offset})");
                return Counters.SetOffsetAsync(offset, HttpContext.RequestAborted);
            }
        }
        #endregion

        #region Part04_ClientServices
        // ICounterServiceClient tells how ICounterService methods map to HTTP methods.
        // As you'll see further, it's used by Replica Service (ICounterService implementation) on the client.
        [BasePath("counter")]
        public interface ICounterServiceClient : IRestEaseReplicaClient
        {
            [Get("get")]
            Task<int> GetAsync(string key, CancellationToken cancellationToken = default);
            [Post("inc")]
            Task IncrementAsync(string key, CancellationToken cancellationToken = default);
            [Post("setOffset")]
            Task SetOffsetAsync(int offset, CancellationToken cancellationToken = default);
        }
        #endregion
    }

    public static class Part04
    {
        #region Part04_CreateXxx
        public static IHost CreateHost()
        {
            var builder = Host.CreateDefaultBuilder();
            builder.ConfigureHostConfiguration(cfg =>
                cfg.AddInMemoryCollection(new Dictionary<string, string>() {{"Environment", "Development"}}));
            builder.ConfigureLogging(logging =>
                logging.ClearProviders().SetMinimumLevel(LogLevel.Information).AddDebug());
            builder.ConfigureServices((b, services) => {
                services.AddFusionWebSocketServer();
                services.AddComputeService<ICounterService, CounterService>();
                services.AddRouting();
                services.AddMvc()
                    .AddApplicationPart(Assembly.GetExecutingAssembly())
                    .AddNewtonsoftJson(options => MemberwiseCopier.CopyMembers(
                        JsonNetSerializer.DefaultSettings,
                        options.SerializerSettings));
            });
            builder.ConfigureWebHost(b => {
                b.UseKestrel();
                b.UseUrls("http://localhost:50050/");
                b.Configure((ctx, app) => {
                    app.UseWebSockets();
                    app.UseRouting();
                    app.UseEndpoints(endpoints => {
                        endpoints.MapControllers();
                        endpoints.MapFusionWebSocketServer();
                    });
                });
            });
            return builder.Build();
        }

        public static IServiceProvider CreateClientServices()
        {
            var services = new ServiceCollection();
            var baseUri = new Uri($"http://localhost:50050/");
            var apiBaseUri = new Uri($"{baseUri}api/");
            services.ConfigureAll<HttpClientFactoryOptions>(options => {
                // Replica Services construct HttpClients using IHttpClientFactory, so this is
                // the right way to make all HttpClients to have BaseAddress = apiBaseUri by default.
                options.HttpClientActions.Add(client => client.BaseAddress = apiBaseUri);
            });
            services.AddFusionWebSocketClient((c, options) => options.BaseUri = baseUri);
            services.AddRestEaseReplicaService<ICounterService, ICounterServiceClient>();
            return services.BuildServiceProvider();
        }
        #endregion

        public static async Task ReplicaService()
        {
            #region Part04_ReplicaService
            using var host = CreateHost();
            await host.StartAsync();
            WriteLine("Host started.");

            using var stopCts = new CancellationTokenSource();
            var cancellationToken = stopCts.Token;

            async Task WatchAsync<T>(string name, IComputed<T> computed)
            {
                for (;;) {
                    WriteLine($"{name}: {computed.Value}, {computed}");
                    await computed.WhenInvalidatedAsync(cancellationToken);
                    WriteLine($"{name}: {computed.Value}, {computed}");
                    computed = await computed.UpdateAsync(false, cancellationToken);
                }
            }

            var services = CreateClientServices();
            var counters = services.GetService<ICounterService>();
            var aComputed = await Computed.CaptureAsync(_ => counters.GetAsync("a"));
            Task.Run(() => WatchAsync(nameof(aComputed), aComputed));
            var bComputed = await Computed.CaptureAsync(_ => counters.GetAsync("b"));
            Task.Run(() => WatchAsync(nameof(bComputed), bComputed));

            await Task.Delay(200);
            await counters.IncrementAsync("a");
            await Task.Delay(200);
            await counters.SetOffsetAsync(10);
            await Task.Delay(200);

            stopCts.Cancel();
            await host.StopAsync();
            #endregion
        }

        public static async Task LiveStateFromReplica()
        {
            #region Part04_LiveStateFromReplica
            using var host = CreateHost();
            await host.StartAsync();
            WriteLine("Host started.");

            var services = CreateClientServices();
            var counters = services.GetService<ICounterService>();
            var stateFactory = services.GetStateFactory();
            using var state = stateFactory.NewLive<string>(
                options => {
                    options.WithUpdateDelayer(TimeSpan.FromSeconds(1)); // 1 second update delay
                    options.Invalidated += state => WriteLine($"{DateTime.Now}: Invalidated, Computed: {state.Computed}");
                    options.Updated     += state => WriteLine($"{DateTime.Now}: Updated, Value: {state.Value}, Computed: {state.Computed}");
                },
                async (state, cancellationToken) => {
                    var counter = await counters.GetAsync("a", cancellationToken);
                    return $"counters.GetAsync(a) -> {counter}";
                });
            await state.UpdateAsync(false); // Ensures the state gets up-to-date value
            await counters.IncrementAsync("a");
            await Task.Delay(2000);
            await counters.SetOffsetAsync(10);
            await Task.Delay(2000);

            await host.StopAsync();
            #endregion
        }
    }
}
