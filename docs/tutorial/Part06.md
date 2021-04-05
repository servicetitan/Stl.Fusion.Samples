# Part 6: Real-time UI in Blazor Apps

You already know about `IState<T>` - it was described in [Part 3](./Part03.md).
It's an abstraction that "tracks" the most current version of some `IComputed<T>`.
There are a few "flavors" of the `IState` - the most important ones are:

* `IMutableState<T>` - in fact, a variable exposed as `IState<T>`
* `ILiveState<T>` - a state that auto-updates once it becomes inconsistent,
  and the update delay is controlled by `UpdateDelayer` provided to it.

You can use these abstractions directly in your Blazor components, but
usually it's more convenient to use `LiveComponentBase<T>` and
`LiveCompontentBase<T, TLocals>` from `Stl.Fusion.Blazor` NuGet package.
I'll describe how they work further, but since the classes
are tiny, the link to their source code might explain it even better:

- [LiveComponentBase.cs](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion.Blazor/LiveComponentBase.cs)
- Its base type - [StatefulComponentBase.cs](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion.Blazor/StatefulComponentBase.cs)

#### StatefulComponentBase&lt;T&gt;

Any `StatefulComponentBase` has `State` property, which can be
any `IState`.

When initialized, it tries to resolve the state via `ServiceProvider` -
unless was already assigned. And in addition to that, it attaches its
own event handler (`StateChanged` delegate - don't confuse it with Blazor's
`StateHasChanged` method) to all `State`'s events (by default):

```cs
protected override void OnInitialized()
{
    State ??= ServiceProvider.GetRequiredService<TState>();
    UntypedState.AddEventHandler(StateEventKind.All, StateChanged);
}
```

And this is how the default `StateChanged` handler looks:

```cs
protected StateEventKind StateHasChangedTriggers { get; set; } = StateEventKind.Updated;

protected StatefulComponentBase()
{
    StateChanged = (state, eventKind) => InvokeAsync(() => {
        if ((eventKind & StateHasChangedTriggers) != 0)
            StateHasChanged();
    });
}
```

As you see, by default any `StatefulComponentBase` triggers `StateHasChanged`
once its `State` gets updated.

Finally, it also disposes the state once the component gets disposed -
unless its `OwnsState` property is set to `false`. And that's nearly all
it does.

#### LiveComponentBase&lt;T&gt;

This class tweaks a behavior of `StatefulComponentBase` to deal `ILiveState<T>`.

This is literally all of its code:

```cs
protected override void OnInitialized()
{
    State ??= StateFactory.NewLive<T>(ConfigureState, (_, ct) => ComputeState(ct), this);
    base.OnInitialized();
}

protected virtual void ConfigureState(LiveState<T>.Options options) { }
protected abstract Task<T> ComputeState(CancellationToken cancellationToken);
```

As you see, it doesn't try to resolve the state via DI container, but
constructs it using `IStateFactory` - and moreover:

- It constructs a state that's computed using its own `ComputeState` method.
  As you remember from [Part 3](./Part03.md), state computation logic is
  always "wrapped" into a compute method - in other words, the `IComputed` instance
  it produces under the hood tracks all the dependencies and gets invalidated
  once any of them does, which triggers `Invalidated` event on a state, and
  consequently, `StateChanged` event on the component. And since we're using
  `ILiveState` here, the state itself will use its `UpdateDelayer` to wait
  a bit and recompute itself using the same `ComputeState` method.
- This state is configured by its own `ConfigureState` method -
  in particular, you can provide its initial value, `UpdateDelayer`, etc.

So to have a component that automatically updates once the output of some
Compute Service (or a set of such services) changes, all you need is to:

- Inherit it from `LiveComponentBase<T>`
- Override its `ComputeState` method
- Possibly, override its `ConfigureState` method.

A good example of such component is `Counter.razor` from "HelloBlazorServer" example -
check out [its source code](https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/src/HelloBlazorServer/Pages/Counter.razor).
Note that it already computes a complex value using two compute methods
(`CounterService.GetCounterAsync` and `GetMomentsAgoAsync`):

```cs
protected override async Task<string> ComputeState(CancellationToken cancellationToken)
{
    var (count, changeTime) = await CounterService.GetCounterAsync().ConfigureAwait(false);
    var momentsAgo = await CounterService.GetMomentsAgoAsync(changeTime).ConfigureAwait(false);
    return $"{count}, changed {momentsAgo}";
}
```

#### LiveComponentBase&lt;T, TLocals&gt;

It's pretty common for live components to have its own (local) state
(e.g. a text entered into a few form fields)
and compute their `State` using some values from this local state -
in other words, to have their `State` dependent on its local values.

There are a few ways to enforce `State` recomputation in such cases:

1. Just call `State.Invalidate()`. You may follow it with
   `State.CancelUpdateDelay()` to trigger the recomputation
   immediately (this is usually desirable for any change
   triggered by user action, and local state changes are almost
   always triggered by user action).
2. Wrap full local state into e.g. `IMutableState<T> Locals` and use
   it in `ComputeState` via `var locals = await Locals.Use()`.
   As you might remember from [Part 3](./Part03.md), `Locals.Use`
   is the same as `Locals.Computed.Use`, and it makes state
   a dependency of what's computed now, so once `Locals` is changed,
   the recomputation of `State` will happen automatically.
   Though if you need to cancel update delay in this case, it's going
   to be a bit more complex.
3. And finally, you can use `LiveComponentBase<T, TLocals>`, which
   takes the best of these two options and implements this :)
   Check out [its source code](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion.Blazor/LiveComponentBase.cs#L23) -
   it is about 15 LOC long and everything is absolutely straightforward there.

## Real-time UI in Server-Side Blazor apps

As you might guess, all you need is to:

- Add your Compute Services to `IServiceProvider` used by ASP.NET Core
- Inherit your own components from `LiveComponentBase<T>` or
  `LiveComponentBase<T, TLocals>`, or just use `ILiveState<T>`
  and / or `IMutableState<T>` there directly to ensure `StateHasChanged`
  is called once the state gets recomputed.

Your server-side web host configuration should include at least these parts:

```cs
public void ConfigureServices(IServiceCollection services)
{
    // Fusion services
    var fusion = services.AddFusion();
    services.AddSingleton(c => new UpdateDelayer.Options() {
        // Default update delayer options 
        Delay = TimeSpan.FromSeconds(0.1),
    });

    // Web
    services.AddRazorPages();
    services.AddServerSideBlazor();
}

// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseStaticFiles();
    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapBlazorHub();
        endpoints.MapFallbackToPage("/_Host");
    });
}

```

## Real-time UI in Blazor WebAssembly apps

If you read about [Replica Services in Part 4](./Part04.md), you
probably already know that WASM case actually isn't that different:

- Server-side should be configured to host Replica Services -
  i.e. its DI container should be able to resolve Compute Services and
  Fusion's `IPublisher`; its middleware chain should include
  Fusion's `WebSocketServer`, and finally, there must be an API controller
  for each Replica Service.
- Client-side should be configured to properly build Replica Service
  clients. And since these clients behave exactly as Compute Services
  they replicate, you can use them the same way you'd use Compute Services
  with Server-Side Blazor.

So your server-side web host configuration should include these parts:

```cs
public void ConfigureServices(IServiceCollection services)
{
    // Fusion services
    services.AddSingleton(new Publisher.Options() { 
        Id = "p-<uniqueId>", 
    });
    var fusion = services.AddFusion();
    var fusionServer = fusion.AddWebSocketServer();

    services.AddRouting();
    // Register Replica Service controllers
    services.AddMvc().AddApplicationPart(Assembly.GetExecutingAssembly());
    services.AddServerSideBlazor();
}

public void Configure(IApplicationBuilder app, ILogger<Startup> log)
{
    if (Env.IsDevelopment()) {
        app.UseWebAssemblyDebugging(); // Only if you need this
    }
    app.UseWebSockets(new WebSocketOptions() {
        KeepAliveInterval = TimeSpan.FromSeconds(30), // You can change this
    });

    // Static files
    app.UseBlazorFrameworkFiles(); // Needed for Blazor WASM

    // API controllers
    app.UseRouting();
    app.UseEndpoints(endpoints => {
        endpoints.MapFusionWebSocketServer();
        endpoints.MapControllers();
        endpoints.MapFallbackToPage("/_Host"); // Typically needed for Blazor WASM
    });
}
```

And your client-side DI container configuration should look as follows:

```cs
public static Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    ConfigureServices(builder.Services, builder);
    builder.RootComponents.Add<App>("app");
    var host = builder.Build();
    var runTask = host.RunAsync();
    
    // Blazor host doesn't start IHostedService-s by default,
    // so let's start them "manually" here
    Task.Run(async () => {
        var hostedServices = host.Services.GetService<IEnumerable<IHostedService>>();
        foreach (var hostedService in hostedServices)
            await hostedService.StartAsync(default);
    });

    return runTask;
}

public static void ConfigureServices(IServiceCollection services, WebAssemblyHostBuilder builder)
{
    var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
    var apiBaseUri = new Uri($"{baseUri}api/"); // You may change this

    var fusion = services.AddFusion();
    var fusionClient = fusion.AddRestEaseClient(
        (c, o) => {
            o.BaseUri = baseUri;
            o.MessageLogLevel = LogLevel.Information;
        }).ConfigureHttpClientFactory(
        (c, name, o) => {
            // This code configures any HttpClient, so if you use a few of them
            // you may add some extra logic to ensure their BaseAddress-es
            // are properly set here
            var isFusionClient = (name ?? "").Contains("FusionClient");
            var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
            o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
        });
    services.AddSingleton(c => new UpdateDelayer.Options() {
        // Default update delayer options 
        Delay = TimeSpan.FromSeconds(0.1),
    });
}
```

## Real-time UI in Blazor Hybrid apps

As you might guess, nothing prevents you from using both of above approaches
to implement Blazor apps that support both Server-Side Blazor (SSB) and
Blazor WebAssembly modes.

All you need is to:

- Ensure all of your Replica Services implement the same interface.
  [Part 4](./Part04.md) explains how to achieve that, but overall,
  you need to implement this interface on Compute Service, the
  controller "exporting" it, and register its client via
  `fusionClient.AddReplicaService<IClientDef, IService>()` call.
- Ensure the server can host Blazor components from the client
  in SSB mode. You need to host Blazor hub + a bit  
  [tweaked _Host.cshtml](https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/src/Blazor/Server/Pages/_Host.cshtml)
  capable of serving the HTML of the Blazor app for both modes.
- Configure server to resolve any `IComputeService` to
  its actual (local) implementation.
- Configure client to resolve any `IComputeService` to
  its Replica Service client.
- And finally, implement something allowing clients to switch
  from SSB to WASM mode and vice versa.

Check out [Blazor Sample](https://github.com/servicetitan/Stl.Fusion.Samples/tree/master/src/Blazor)
to see how all of this works together.

#### [Next: Part 7 &raquo;](./Part07.md) | [Tutorial Home](./README.md)

