# Part 4: Replica Services

Video covering this part:

[<img src="./img/Part4-Screenshot.jpg" width="200"/>](https://youtu.be/_wFhi11Eb0o)

Replica Services are remote proxies of Compute Services that take
the behavior of `IComputed<T>` into account to be more efficient
than identical web API clients.

Namely:

- They similarly back the result to any call with `IComputed<T>` that mimics
  matching `IComputed<T>` on the server side. So client-side Replica Services
  can be used in other client-side Compute Services - and as you might guess,
  invalidation of a server-side dependency will trigger invalidation of
  its client-side replica (`IComputed<T>` too), which in turn will invalidate
  every client-side computed that uses it.
- They similarly cache consistent replicas. In other words, Replica Service
  won't make a remote call in case a *consistent* replica is still available.
  So it's exactly the same behavior as for Compute Services if we replace
  the "computation" with "RPC call".

It's more or less obvious how Replica Services create initial versions of replicas -
e.g. they can simply call an HTTP endpoint to get a copy of `IComputed<T>`.
But how do they get invalidation notifications?

Both invalidation and update messages are currently delivered via
additional WebSocket-based `Publisher`-`Replicator` channel; the connection is
made by the client when the first replica is created there; the channel
is used to serve all of such notifications from a given `Publisher` (~ server)
further.

Resiliency (reconnection on disconnect, auto-refresh of state of
all replicas in case of reconnect, etc.) is bundled both into the implementation
and into the protocol (e.g. that's the main reason for any `IComputed<T>` to have
`Version` property).

Finally, Replica Services are just interfaces. They typically
declare all the methods of a Compute Service they "mimic".
The interfaces are needed solely to describe how method calls should be
mapped to corresponding HTTP endpoints.

Fusion implements Replica Service interfaces in runtime - currently relying
on [RestEase](https://github.com/canton7/RestEase)
and its own
[Castle.DynamicProxy](http://www.castleproject.org/projects/dynamicproxy/)-based proxies,
even though in future there might be other implementations.

The sequence diagram below shows what happens when a regular Web API client
(e.g. a regular RestEase client) processes the call.
"Web API" is controller forwarding the call to the underlying
service (`GreetingService` in this example):

[<img src="./img/WebApi-Regular.jpg" width="600"/>](./img/WebApi-Regular.jpg)

And that's a similar diagram showing what happens when a Replica Service
processes the call + invalidates & updates the value later:

[<img src="./img/WebApi-Fusion.jpg" width="600"/>](./img/WebApi-Fusion.jpg)

Gantt chart for this process could look as follows:

[<img src="./img/ComputedReplica-Gantt.jpg" width="600"/>](./img/ComputedReplica-Gantt.jpg)

Ok, let's write some code to learn how it works. Unfortunately this time the amount of
code is going to explode a bit - that's mostly due to the fact we'll need a web server
hosting Compute Service itself, a controller publishing its invocable endpoints, etc.

1. Common interface (don't run this code yet):

``` cs --editable false --region Part04_CommonServices --source-file Part04.cs
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
```

2. Web host services (don't run this code yet):

``` cs --editable false --region Part04_HostServices --source-file Part04.cs
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
        using (Computed.Invalidate())
            GetAsync(key, default).Ignore();
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
[ApiController, JsonifyErrors]
public class CounterController : ControllerBase
{
    private ICounterService Counters { get; }

    public CounterController(ICounterService counterService)
        => Counters = counterService;

    // Publish ensures GetAsync output is published if publication was requested by the client:
    // - Publication is created
    // - Its Id is shared in response header.
    [HttpGet("get"), Publish]
    public Task<int> GetAsync(string key)
    {
        key ??= ""; // Empty value is bound to null value by default
        WriteLine($"{GetType().Name}.{nameof(GetAsync)}({key})");
        return Counters.GetAsync(key, HttpContext.RequestAborted);
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
```

3. Client services (don't run this code yet):

``` cs --editable false --region Part04_ClientServices --source-file Part04.cs
// ICounterServiceClient tells how ICounterService methods map to HTTP methods.
// As you'll see further, it's used by Replica Service (ICounterService implementation) on the client.
[BasePath("counter")]
public interface ICounterServiceClient
{
    [Get("get")]
    Task<int> GetAsync(string key, CancellationToken cancellationToken = default);
    [Post("inc")]
    Task IncrementAsync(string key, CancellationToken cancellationToken = default);
    [Post("setOffset")]
    Task SetOffsetAsync(int offset, CancellationToken cancellationToken = default);
}
```

4. `CreateHost` and `CreateClientServices` methods (don't run this code yet):

``` cs --editable false --region Part04_CreateXxx --source-file Part04.cs
public static IHost CreateHost()
{
    var builder = Host.CreateDefaultBuilder();
    builder.ConfigureHostConfiguration(cfg =>
        cfg.AddInMemoryCollection(new Dictionary<string, string>() { { "Environment", "Development" } }));
    builder.ConfigureLogging(logging =>
        logging.ClearProviders().SetMinimumLevel(LogLevel.Information).AddDebug());
    builder.ConfigureServices((b, services) =>
    {
        services.AddFusion(f =>
        {
            f.AddWebSocketServer();
            f.AddComputeService<ICounterService, CounterService>();
        });
        services.AddRouting();
        services.AddControllers().AddApplicationPart(Assembly.GetExecutingAssembly());
    });
    builder.ConfigureWebHost(b =>
    {
        b.UseKestrel();
        b.UseUrls("http://localhost:50050/");
        b.Configure((ctx, app) =>
        {
            app.UseWebSockets();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
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
    services.ConfigureAll<HttpClientFactoryOptions>(options =>
    {
        // Replica Services construct HttpClients using IHttpClientFactory, so this is
        // the right way to make all HttpClients to have BaseAddress = apiBaseUri by default.
        options.HttpClientActions.Add(client => client.BaseAddress = apiBaseUri);
    });
    var fusion = services.AddFusion();
    var fusionClient = fusion.AddRestEaseClient((c, options) => options.BaseUri = baseUri);
    fusionClient.AddReplicaService<ICounterService, ICounterServiceClient>();
    return services.BuildServiceProvider();
}
```

And finally, we're ready to try our Replica Service:

``` cs --region Part04_ReplicaService --source-file Part04.cs
using var host = CreateHost();
await host.StartAsync();
WriteLine("Host started.");

            using var stopCts = new CancellationTokenSource();
var cancellationToken = stopCts.Token;

async Task WatchAsync<T>(string name, IComputed<T> computed)
{
    for (; ; )
    {
        WriteLine($"{name}: {computed.Value}, {computed}");
        await computed.WhenInvalidatedAsync(cancellationToken);
        WriteLine($"{name}: {computed.Value}, {computed}");
        computed = await computed.UpdateAsync(false, cancellationToken);
    }
}

var services = CreateClientServices();
var counters = services.GetRequiredService<ICounterService>();
var aComputed = await Computed.CaptureAsync(_ => counters.GetAsync("a"));
Task.Run(() => WatchAsync(nameof(aComputed), aComputed)).Ignore();
var bComputed = await Computed.CaptureAsync(_ => counters.GetAsync("b"));
Task.Run(() => WatchAsync(nameof(bComputed), bComputed)).Ignore();

await Task.Delay(200);
await counters.IncrementAsync("a");
await Task.Delay(200);
await counters.SetOffsetAsync(10);
await Task.Delay(200);

stopCts.Cancel();
await host.StopAsync();
```

The output:

```text
Host started.
CounterController.GetAsync(a)
GetAsync(a)
aComputed: 0, ReplicaClientComputed`1(Intercepted:ICounterServiceProxy.GetAsync(a, System.Threading.CancellationToken) @4f, State: Consistent)
CounterController.GetAsync(b)
GetAsync(b)
bComputed: 0, ReplicaClientComputed`1(Intercepted:ICounterServiceProxy.GetAsync(b, System.Threading.CancellationToken) @6j, State: Consistent)
CounterController.IncrementAsync(a)
IncrementAsync(a)
aComputed: 0, ReplicaClientComputed`1(Intercepted:ICounterServiceProxy.GetAsync(a, System.Threading.CancellationToken) @4f, State: Invalidated)
GetAsync(a)
aComputed: 1, ReplicaClientComputed`1(Intercepted:ICounterServiceProxy.GetAsync(a, System.Threading.CancellationToken) @2m, State: Consistent)
CounterController.SetOffsetAsync(10)
SetOffsetAsync(10)
bComputed: 0, ReplicaClientComputed`1(Intercepted:ICounterServiceProxy.GetAsync(b, System.Threading.CancellationToken) @6j, State: Invalidated)
aComputed: 1, ReplicaClientComputed`1(Intercepted:ICounterServiceProxy.GetAsync(a, System.Threading.CancellationToken) @2m, State: Invalidated)
GetAsync(a)
GetAsync(b)
aComputed: 11, ReplicaClientComputed`1(Intercepted:ICounterServiceProxy.GetAsync(a, System.Threading.CancellationToken) @2n, State: Consistent)
bComputed: 10, ReplicaClientComputed`1(Intercepted:ICounterServiceProxy.GetAsync(b, System.Threading.CancellationToken) @29, State: Consistent)
```

So Replica Service does its job &ndash; it perfectly mimics the underlying Compute Service!

Notice that `CounterController` methods are invoked just once for a given set of arguments &ndash;
that's because while some replica exists, Replica Services uses it to update its value, i.e. the updates
are requested and delivered via WebSocket channel.

As you might guess, the controller we were using here is a regular Web API controller.
If you're curious whether it's possible to call its methods without Fusion - yes, it is.
**So every Fusion endpoint is also a regular Web API endpoint!** The proof:

[<img src="./img/SwaggerPost.jpg" width="600"/>](https://www.youtube.com/watch?v=jYVe5yd0xuQ&t=4173s)

Now, let's show that client-side `LiveState<T>` can use Replica Service
to "observe" the output of server-side Compute Service. The code below
is almost the same as you saw in previous part showcasing `LiveState<T>`,
but it uses Replica Service instead of Computed Service.

``` cs --region Part04_LiveStateFromReplica --source-file Part04.cs
using var host = CreateHost();
await host.StartAsync();
WriteLine("Host started.");

var services = CreateClientServices();
var counters = services.GetRequiredService<ICounterService>();
var stateFactory = services.StateFactory();
            using var state = stateFactory.NewLive<string>(
                options =>
                {
                    options.WithUpdateDelayer(TimeSpan.FromSeconds(1)); // 1 second update delay
                    options.EventConfigurator += state1 =>
                    {
                        // A shortcut to attach 3 event handlers: Invalidated, Updating, Updated
                        state1.AddEventHandler(StateEventKind.All,
                            (s, e) => WriteLine($"{DateTime.Now}: {e}, Value: {s.Value}, Computed: {s.Computed}"));
                    };
                },
                async (state, cancellationToken) =>
                {
                    var counter = await counters.GetAsync("a", cancellationToken);
                    return $"counters.GetAsync(a) -> {counter}";
                });
await state.UpdateAsync(false); // Ensures the state gets up-to-date value
await counters.IncrementAsync("a");
await Task.Delay(2000);
await counters.SetOffsetAsync(10);
await Task.Delay(2000);

await host.StopAsync();
```

The output:

```text
Host started.
10/2/2020 6:27:48 AM: Updated, Value: , Computed: StateBoundComputed`1(FuncLiveState`1(#38338487) @26, State: Consistent)
10/2/2020 6:27:48 AM: Invalidated, Value: , Computed: StateBoundComputed`1(FuncLiveState`1(#38338487) @26, State: Invalidated)
10/2/2020 6:27:48 AM: Updating, Value: , Computed: StateBoundComputed`1(FuncLiveState`1(#38338487) @26, State: Invalidated)
CounterController.GetAsync(a)
GetAsync(a)
10/2/2020 6:27:48 AM: Updated, Value: counters.GetAsync(a) -> 0, Computed: StateBoundComputed`1(FuncLiveState`1(#38338487) @4a, State: Consistent)
CounterController.IncrementAsync(a)
IncrementAsync(a)
10/2/2020 6:27:48 AM: Invalidated, Value: counters.GetAsync(a) -> 0, Computed: StateBoundComputed`1(FuncLiveState`1(#38338487) @4a, State: Invalidated)
10/2/2020 6:27:49 AM: Updating, Value: counters.GetAsync(a) -> 0, Computed: StateBoundComputed`1(FuncLiveState`1(#38338487) @4a, State: Invalidated)
GetAsync(a)
10/2/2020 6:27:50 AM: Updated, Value: counters.GetAsync(a) -> 1, Computed: StateBoundComputed`1(FuncLiveState`1(#38338487) @6h, State: Consistent)
CounterController.SetOffsetAsync(10)
SetOffsetAsync(10)
10/2/2020 6:27:50 AM: Invalidated, Value: counters.GetAsync(a) -> 1, Computed: StateBoundComputed`1(FuncLiveState`1(#38338487) @6h, State: Invalidated)
10/2/2020 6:27:51 AM: Updating, Value: counters.GetAsync(a) -> 1, Computed: StateBoundComputed`1(FuncLiveState`1(#38338487) @6h, State: Invalidated)
GetAsync(a)
10/2/2020 6:27:51 AM: Updated, Value: counters.GetAsync(a) -> 11, Computed: StateBoundComputed`1(FuncLiveState`1(#38338487) @ap, State: Consistent)
10/2/2020 6:27:52 AM: Invalidated, Value: counters.GetAsync(a) -> 11, Computed: StateBoundComputed`1(FuncLiveState`1(#38338487) @ap, State: Invalidated)
```

As you might guess, this is exactly the logic out Blazor samples use to update
the UI in real time. Moreover, we similarly make Compute Services and Replica Services
there to implement the same common interfaces - and that's precisely what allows
use to have the same UI components working in WASM and Server-Side Blazor mode:

- When UI components are rendered on the server side, they pick server-side
  Compute Services from host's `IServiceProvider` as implementation of
  `IWhateverService`. Replicas aren't needed there, because everything is local.
- And when the same UI components are rendered on the client, they pick
  Replica Services as `IWhateverService` from the client-side IoC container,
  and that's what makes any `IState<T>` to update in real time there, which
  in turn makes UI components to re-render.

**That's pretty much it - now you learned all key features of Fusion.**
There are details, of course, and the rest of the tutorial is mostly about them.

#### [Next: Part 5 &raquo;](./Part05.md) | [Tutorial Home](./README.md)

