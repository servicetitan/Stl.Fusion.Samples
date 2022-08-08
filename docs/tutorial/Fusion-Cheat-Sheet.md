# Fusion Cheat Sheet

## Compute Services

Compute service interface:
```cs
// IComputeService is just an optional tagging interface.
// Nevertheless, we highly recommend to "implement" it -
// it allows you to use a few extension methods, such as
// .GetServices() and .GetCommander().
public interface ICartService : IComputeService
{
    // When applied to interface method, this attribute 
    // is "inherited" by the implementation
    [ComputeMethod]
    Task<List<Order>> GetOrders(long cartId, CancellationToken cancellationToken = default);
}
```
Compute service implementation:
```cs
public class CartService : ICartService 
{
    // The method must be virtual + return Task<T>
    public virtual async Task<List<Order>> GetOrders(long cartId, CancellationToken cancellationToken)
    {
        // Implementation goes here
    }
}    
```

Add invalidation logic (after any code that changes the result of what's invalidated inside the `using` block below):
```cs
using (Computed.Invalidate()) {
    // Whatever compute method calls you make inside this block
    // are invalidating calls. Instead of running the actual method 
    // code they invalidate the result of this call.
    // They always complete synchronously, and you can pass
    // "default" instead of any CancellationToken here.
    _ = GetOrders(cartId, default);
}    
```

Register compute service:
```cs
fusion = services.AddFusion(); // services is IServiceCollection
fusion.AddComputeService<ICartService, CartService>();
```

## Replica Services

Add controller:
```cs
[Route("api/[controller]/[action]")]
[ApiController, JsonifyErrors, UseDefaultSession]
public class CartController : ControllerBase, ICartService
{
    private readonly ICartService _cartService;
    private readonly ICommander _commander;

    public CartController(ICartService service, ICommander commander) 
    {
        _service = service;
        _commander = commander;
    }    

    [HttpGet, Publish]
    public Task<List<Order>> GetOrders(long cartId, CancellationToken cancellationToken)
        => _service.GetOrders(cartId, cancellationToken);
}
```

Add client definition:
```cs
[BasePath("cart")]
public interface ICartClientDef
{
    [Get(nameof(GetOrders))]
    Task<List<Order>> GetOrders(long cartId, CancellationToken cancellationToken);
}
```

Configure Fusion client (this has to be done once in a code that configures client-side `IServiceProvider`):
```cs
var baseUri = new Uri("http://localhost:5005");
var apiBaseUri = new Uri($"{baseUri}api/");

var fusion = services.AddFusion();
var fusionClient = fusion.AddRestEaseClient(
    client => {
        client.ConfigureWebSocketChannel(_ => new() { BaseUri = baseUri });
        client.ConfigureHttpClient((_, name, o) => {
            var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
            var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
            o.HttpClientActions.Add(httpClient => httpClient.BaseAddress = clientBaseUri);
        });
    });
```

Register Replica Service:
```cs
// After "var fusionClient = ..."
fusionClient.AddReplicaService<ICartService, ICartClientDef>();
```

Use Replica Service:
```cs
// Just call it the same way you call the original one.
// Any calls that are expected to produce the same result
// as the previous call with the same arguments will
// be resolved via locally cached IComputed.
// When a IComputed gets invalidated on the server,
// Fusion will invalidate its replica on every client.
```

## Commander

Declare command type:
```cs
// You don't have to use records, but we recommend to use
// immutable types for commands and outputs of compute methods
public record UpdateCartCommand(long CartId, Dictionary<long, long?> Updates) 
    : ICommand<Unit> // Unit is command's return type; you can use any other
{
    // Compatibility: Newtonsoft.Json needs this constructor to deserialize the record
    public UpdateCartCommand() : this(0, null!) { }
}
```

Add command handler in the compute service interface:
```cs
public interface ICartService : IComputeService
{
    // ...
    [CommandHandler] // This attribute is also "inherited" by the impl.
    Task<Unit> UpdateCart(UpdateCartCommand command, CancellationToken cancellationToken = default);
```

Add command handler implementation:
```cs
public class CartService : ICartService 
{
    // Must be virtual + return Task<T> for ICommand<T>;
    // Command must be its first argument; other arguments are resolved
    // from DI container - except CancellationToken, which is 
    // passed directly.
    public virtual Task<Unit> UpdateCart(UpdateCartCommand command, CancellationToken cancellationToken) 
    {
        if (Computed.IsInvalidating()) {
            // Write the invalidation logic for this command here.
            //
            // A set of command handlers registered by Fusion will
            // "retry" this handler inside the invalidation block
            // once its "normal" logic completes successfully.
            // Moreover, they'll run this block on every node on the 
            // cluster if you use multi-host invalidation.
            return default;
        }

        // Command handler code goes here
    }

```

Register command handler:
```
// Nothing is needed for handlers declared inside compute services
```

## Exposing commands to the client via Replica Services

Add controller method for the command:
```cs
public class CartController : ControllerBase, ICartService
    // Always use [HttpPost] and [FromBody]
    [HttpPost]
    public Task<Unit> UpdateCart([FromBody] UpdateCartCommand command, CancellationToken cancellationToken)
        // You can also call corresponding service method directly here,
        // but if you're using CommanderOptions.AllowDirectCommandHandlerCalls = false,
        // only this "style" of command invocation will work.
        // For the note, they work the same, i.e. when a command handler
        // is invoked directly, the call is still routed via ICommander.
        => _commander.Call(command, cancellationToken);
```        

Add command handler in the client definition interface:
```cs
public interface ICartClientDef 
{
    // Always use [Post] and [Body] here
    [Post(nameof(UpdateCart))]
    Task<Unit> UpdateCart([Body] UpdateCartCommand command, CancellationToken cancellationToken);
```        

Register client-side command handler:
```
// Nothing is needed for handlers declared inside replica services
```

## Working with `IComputed`

Capture:
```cs
var computed = await Computed.Capture(ct => service.ComputeMethod(args, ct), cancellationToken);
```

Check whether `IComputed` is still consistent:
```cs
if (computed.IsConsistent()) {
    // ...
}
```

Await for invalidation:
```cs
// Always pass CancellationToken here, otherwise you'll
// end up with a memory leak due to growing number of
// event handler registrations. They'll be gone on
// invalidation, of course, but what if it never happens?
await computed.WhenInvalidated(cancellationToken);
// Or
computed.Invalidated += c => Console.WriteLine("Invalidated!");
```

To be continued.

#### [&gt; Back to Tutorial](./README.md) | [Documentation Home](../index.md)
