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
using Stl.Async;
using Stl.Fusion;
using Stl.Fusion.Client;
using Stl.Fusion.Server;
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
            Task<int> Get(string key, CancellationToken cancellationToken = default);
            Task Increment(string key, CancellationToken cancellationToken = default);
            Task SetOffset(int offset, CancellationToken cancellationToken = default);
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
            public virtual async Task<int> Get(string key, CancellationToken cancellationToken = default)
            {
                WriteLine($"{nameof(Get)}({key})");
                var offset = await _offset.Use(cancellationToken);
                return offset + (_counters.TryGetValue(key, out var value) ? value : 0);
            }

            public Task Increment(string key, CancellationToken cancellationToken = default)
            {
                WriteLine($"{nameof(Increment)}({key})");
                _counters.AddOrUpdate(key, k => 1, (k, v) => v + 1);
                using (Computed.Invalidate())
                    Get(key, default).Ignore();
                return Task.CompletedTask;
            }

            public Task SetOffset(int offset, CancellationToken cancellationToken = default)
            {
                WriteLine($"{nameof(SetOffset)}({offset})");
                _offset.Value = offset;
                return Task.CompletedTask;
            }
        }

        // We need Web API controller to publish the service
        [Route("api/[controller]/[action]")]
        [ApiController, JsonifyErrors]
        public class CounterController : ControllerBase
        {
            private ICounterService Counters { get; }

            public CounterController(ICounterService counterService)
                => Counters = counterService;

            // Publish ensures GetAsync output is published if publication was requested by the client:
            // - Publication is created
            // - Its Id is shared in response header.
            [HttpGet, Publish]
            public Task<int> Get(string key)
            {
                key ??= ""; // Empty value is bound to null value by default
                WriteLine($"{GetType().Name}.{nameof(Get)}({key})");
                return Counters.Get(key, HttpContext.RequestAborted);
            }

            [HttpPost]
            public Task Increment(string key)
            {
                key ??= ""; // Empty value is bound to null value by default
                WriteLine($"{GetType().Name}.{nameof(Increment)}({key})");
                return Counters.Increment(key, HttpContext.RequestAborted);
            }

            [HttpPost]
            public Task SetOffset(int offset)
            {
                WriteLine($"{GetType().Name}.{nameof(SetOffset)}({offset})");
                return Counters.SetOffset(offset, HttpContext.RequestAborted);
            }
        }
        #endregion

        #region Part04_ClientServices
        // ICounterServiceClient tells how ICounterService methods map to HTTP methods.
        // As you'll see further, it's used by Replica Service (ICounterService implementation) on the client.
        [BasePath("counter")]
        public interface ICounterServiceClient
        {
            [Get("get")]
            Task<int> Get(string key, CancellationToken cancellationToken = default);
            [Post("increment")]
            Task Increment(string key, CancellationToken cancellationToken = default);
            [Post("setOffset")]
            Task SetOffset(int offset, CancellationToken cancellationToken = default);
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
                services.AddFusion(f => {
                    f.AddWebServer();
                    f.AddComputeService<ICounterService, CounterService>();
                });
                services.AddRouting();
                services.AddControllers().AddApplicationPart(Assembly.GetExecutingAssembly());
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
            var fusion = services.AddFusion();
            var fusionClient = fusion.AddRestEaseClient((c, options) => options.BaseUri = baseUri);
            fusionClient.AddReplicaService<ICounterService, ICounterServiceClient>();
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

            async Task Watch<T>(string name, IComputed<T> computed)
            {
                for (;;) {
                    WriteLine($"{name}: {computed.Value}, {computed}");
                    await computed.WhenInvalidated(cancellationToken);
                    WriteLine($"{name}: {computed.Value}, {computed}");
                    computed = await computed.Update(cancellationToken);
                }
            }

            var services = CreateClientServices();
            var counters = services.GetRequiredService<ICounterService>();
            var aComputed = await Computed.Capture(_ => counters.Get("a"));
            Task.Run(() => Watch(nameof(aComputed), aComputed)).Ignore();
            var bComputed = await Computed.Capture(_ => counters.Get("b"));
            Task.Run(() => Watch(nameof(bComputed), bComputed)).Ignore();

            await Task.Delay(200);
            await counters.Increment("a");
            await Task.Delay(200);
            await counters.SetOffset(10);
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
            var counters = services.GetRequiredService<ICounterService>();
            var stateFactory = services.StateFactory();
            using var state = stateFactory.NewComputed<string>(
                options => {
                    options.UpdateDelayer = new UpdateDelayer(1.0); // 1 second update delay
                    options.EventConfigurator += state1 => {
                        // A shortcut to attach 3 event handlers: Invalidated, Updating, Updated
                        state1.AddEventHandler(StateEventKind.All,
                            (s, e) => WriteLine($"{DateTime.Now}: {e}, Value: {s.Value}, Computed: {s.Computed}"));
                    };
                },
                async (state, cancellationToken) => {
                    var counter = await counters.Get("a", cancellationToken);
                    return $"counters.GetAsync(a) -> {counter}";
                });
            await state.Update(); // Ensures the state gets up-to-date value
            await counters.Increment("a");
            await Task.Delay(2000);
            await counters.SetOffset(10);
            await Task.Delay(2000);

            await host.StopAsync();
            #endregion
        }
    }
}
