# HelloCart Sample Overview

`HelloCart` shows how to implement a simple API
by starting from a toy (but still Fusion-based) version
and transition to a production-ready implementation
in a few iterations.

The API it implements is defined in 
[Abstractions.cs](https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/src/HelloCart/Abstractions.cs).

There are two immutable model types:

```cs
public record Product : IHasId<string>
{
    public string Id { get; init; } = "";
    public decimal Price { get; init; } = 0;
}

public record Cart : IHasId<string>
{
    public string Id { get; init; } = "";
    public ImmutableDictionary<string, decimal> Items { get; init; } = ImmutableDictionary<string, decimal>.Empty;
}
```

The immutability and `IHasId<string>` implementation are totally 
optional - use them mostly because I love the idea to show
all the constraints explicitly.

We're also going to implement two services:

```cs
public interface IProductService
{
    [CommandHandler]
    Task EditAsync(EditCommand<Product> command, CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<Product?> FindAsync(string id, CancellationToken cancellationToken = default);
}

public interface ICartService
{
    [CommandHandler]
    Task EditAsync(EditCommand<Cart> command, CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<Cart?> FindAsync(string id, CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<decimal> GetTotalAsync(string id, CancellationToken cancellationToken = default);
}
```

If we ignore `[CommandHandler]` and `[ComputeMethod]` for now,
it's a tiny CRUD CQRS-style API, where `EditCommand<T>`
is used to implement creation, update, and deletion:

```cs
public record EditCommand<TValue>(string Id, TValue? Value = null) : ICommand<Unit>
    where TValue : class, IHasId<string>
{
    public EditCommand(TValue value) : this(value.Id, value) { }
    // Needed just to make JSON deserialization work for this type:
    public EditCommand() : this("", null) { } 
}
```

## How are we going to use Fusion here?

Imagine we build a UI. Our UI code runs on the client
(think Blazor WebAssembly), and for simplicity, we care
just about a single thing: **the total cost of items in
user's cart**. We want to update this cost in real-time
once anything impacting it changes.

This is why `ICartService.GetTotalAsync` exists - this
method is expected to return the right total. 
But... Looks like there is nothing in our API that could
tell the client that total for the specific cart changes, 
right?

The right answer to this question is "Wrong! It's a 
Fusion API, and every read endpoint of a Fusion API is
capable of doing exactly this!". But before we jump to
the details, let's think how we'd implement the same 
behavior without Fusion.

One possible option is:
1. Add SignalR hub on server
2. Make it broadcast every command to every client
3. Make clients to maintain their own copies of
   their carts by "watching" for `EditCommand<Product>`
   and `EditCommand<Cart>` and update cart's content
   and total.

Note that point #3 already implies that you have to add
a fair amount of code on the client side:
- A very minimal implementation should at least discard
  the commands that aren't related to the cart you're 
  watching. Remember, server broadcasts every command,
  so the client may see `EditCommand("apple")`, but 
  a `Product` with `Id == "apple"` might not exist in the
  user's cart on this client, right?
- Once the client recognizes above command as "relevant",
  it should somehow update the cart. One option is to 
  update it right on the client, but it requires us
  to have a separate version of cart update logic running 
  on the client, which isn't perfect from DRY standpoint. 
- So if you're a big fan of DRY & minimalism, 
  you might prefer to request a new cart directly from 
  server once you see a command impacting it.
  On a downside, it will definitely increase the load on
  server - fetching the cart requires fetching its items,
  products, etc... So you almost certainly need to cache 
  carts there.

And that's just the beginning of our problems - the implementation
described above:
1. Allows everyone to watch everyone else's purchases. 
   This might be fine in some countries, but... Most of
   these countries are still living in pre-internet age,
   so it's a fictional scenario even there.
   
   But even taking ethics and security aside,
   retransmitting every command to every client means
   you'll see `O(ClientCount^2)` packet rate on server,
   which consequently means this implementation also won't scale.
   You absolutely need to filter commands on the server side
   just because of this. 

2. Ok, we need to filter our commands on the server side.
   Let's add an extra logic to `Edit<Product>` handler
   that finds every cart this product is added to
   and notifies every customer watching these carts.

   Here you realize you need a pub-sub to implement
   this - i.e. your clients will have to subscribe to and
   unsubscribe from topics like `"cart-[cartId]"` to 
   watch for... 
   Wait, are we still going to send `Edit<Product>` commands
   to these topics, or we better go with a separate 
   model for these notifications? And if yes - we'll definitely
   need a separate logic to process these...

3. What if your client temporarily loses its connection 
   to server? Remember that it needs to know precise
   cart content to properly update it on every command 
   or change notification.
   So you need a logic that will refresh the cart
   once reconnection happens, right?

4. Most of real-time messaging APIs 
   [don't provide strong guarantees for message ordering](https://github.com/dotnet/aspnetcore/issues/9240) - especially for messages
   sent to different topics / channels, and any *real* real-time
   app uses a number of such channels. Moreover, if you send requests
   via regular HTTP API to the same server, the order of
   these responses and the order of messages you get via SignalR
   can differ from their order on server side. 
   In other words, you might receive `EditCommand("apple", 5)` 
   message first (which sets "apple" price to 5),
   and after that get the "current" cart content, which is 
   already an outdated now - you requested it few second ago 
   due to reconnect, and this request was completed by 
   server before processing `EditCommand("apple", 5)` command,
   but the message describing this command somehow got
   to the client faster.
   In other words, you need to take a number of extra steps
   to ensure the state exposed by your client is still
   eventually consistent (i.e. eventually the client will display
   the right cart content and the right total).

5. "Real real-time app" also means you'll have multiple 
   servers. Usually each client talks with just one of them,
   but changes may happen on any other, so your servers need
   to somehow announce these changes to their peers.
   And if you think how to implement this, you'll quickly 
   conclude that you need **one more flavor of the same 
   protocol, but now for cross-server change announcements,
   and moreover, these servers have to implement reactions
   to these events too!**

That's not the full list, but the gist is: 
**as usual, the problem is much more complex than it initially seems**.
And even if we leave all the technical difficulties aside,
the straightforward implementation of such an architecture
makes you to violate 
[DRY](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself) and 
[SRP](https://en.wikipedia.org/wiki/Single-responsibility_principle) 
multiple times. 

If you're not convinced yet that all if this doesn't look good,
check out my other post covering another similar scenario:
["How Similar Is Fusion to SignalR?"](https://medium.com/swlh/how-similar-is-stl-fusion-to-signalr-e751c14b70c3?source=friends_link&sk=241d5293494e352f3db338d93c352249)


## Can we do better than than?

Yes, and that's exactly what I'm going to talk about further.
Let's launch `HelloCart` and see what it does:

![](./img/Samples-HelloCart.gif)

Once you select the API implementation, the sample uses it
to create 3 products and 2 carts:

```cs
// Code from AppBase.cs
public virtual async Task InitializeAsync()
{
    var pApple = new Product { Id = "apple", Price = 2M };
    var pBanana = new Product { Id = "banana", Price = 0.5M };
    var pCarrot = new Product { Id = "carrot", Price = 1M };
    ExistingProducts = new [] { pApple, pBanana, pCarrot };
    foreach (var product in ExistingProducts)
        await HostProductService.EditAsync(new EditCommand<Product>(product));

    var cart1 = new Cart() { Id = "cart:apple=1,banana=2",
        Items = ImmutableDictionary<string, decimal>.Empty
            .Add(pApple.Id, 1)
            .Add(pBanana.Id, 2)
    };
    var cart2 = new Cart() { Id = "cart:banana=1,carrot=1",
        Items = ImmutableDictionary<string, decimal>.Empty
            .Add(pBanana.Id, 1)
            .Add(pCarrot.Id, 1)
    };
    ExistingCarts = new [] { cart1, cart2 };
    foreach (var cart in ExistingCarts)
        await HostCartService.EditAsync(new EditCommand<Cart>(cart));
}
```

Once this is done, it creates a set of background tasks
watching for changes made to every one of them:

```cs
public Task WatchAsync(CancellationToken cancellationToken = default)
{
    var tasks = new List<Task>();
    foreach (var product in ExistingProducts)
        tasks.Add(WatchProductAsync(product.Id, cancellationToken));
    foreach (var cart in ExistingCarts)
        tasks.Add(WatchCartTotalAsync(cart.Id, cancellationToken));
    return Task.WhenAll(tasks);
}

public async Task WatchProductAsync(string productId, CancellationToken cancellationToken = default)
{
    var productService = WatchServices.GetRequiredService<IProductService>();
    var computed = await Computed.CaptureAsync(ct => productService.FindAsync(productId, ct), cancellationToken);
    while (true) {
        WriteLine($"  {computed.Value}");
        await computed.WhenInvalidatedAsync(cancellationToken);
        computed = await computed.UpdateAsync(false, cancellationToken);
    }
}

public async Task WatchCartTotalAsync(string cartId, CancellationToken cancellationToken = default)
{
    var cartService = WatchServices.GetRequiredService<ICartService>();
    var computed = await Computed.CaptureAsync(ct => cartService.GetTotalAsync(cartId, ct), cancellationToken);
    while (true) {
        WriteLine($"  {cartId}: total = {computed.Value}");
        await computed.WhenInvalidatedAsync(cancellationToken);
        computed = await computed.UpdateAsync(false, cancellationToken);
    }
}
```

Let's postpone the discussion of above code for now. 
The only remark I want to make at this point is that 
`WatchAsync` is started in fire-and-forgot fashion 
in [`Program.cs`, line ~50](https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/src/HelloCart/Program.cs#L52).

Finally, the looped section in `Program.cs` starts to
as you to enter `[productId]=[price]` expression, parses it,
and sends the following command:

```cs
var command = new EditCommand<Product>(product with { Price = price });
await app.ClientProductService.EditAsync(command);
// You can run absolutely identical action with:
// await app.ClientServices.Commander().CallAsync(command);
```

As you see, this call triggers "watcher" task for the product
you change, but interestingly, for cart's total too. 
And not just for a single cart, but for any cart that
contains the product we modify - you may see this by
typing `banana=X` expression ("banana" is contained
in both carts).

So how does this work?

Let's look at the very basic implementation (`v1`)
of `IProductService` and `ICartService`.

## Version 1: ConcurrentDictionary-based API implementation

Code: [src/HelloCart/v1](https://github.com/servicetitan/Stl.Fusion.Samples/tree/master/src/HelloCart/v1)

> ☝ This is the most complex, but also the most important part
> of this document, because it explains nearly all key abstractions.
> Please be patient and read it carefully 🙏

First, check out `InMemoryProductService` there. 
You might notice just a few unusual things there:
1. All of its API methods (declared in `IProductService`) are 
   marked as `virtual`
2. `EditAsync` contains a bit unusual piece of code:
   ```cs
    if (Computed.IsInvalidating()) {
        FindAsync(productId, default).Ignore();
        return Task.CompletedTask;
    }
   ```

Everything else looks absolutely normal. 

The same is equally applicable to `InMemoryCartService`:
1. All of its API methods (declared in `ICartService`) are 
   marked as `virtual`
2. `EditAsync` contains a bit unusual piece of code:
   ```cs
    if (Computed.IsInvalidating()) {
        FindAsync(cartId, default).Ignore();
        return Task.CompletedTask;
    }
   ```

Finally, let's look at the code that registers these
services in IoC container:
```cs
public class AppV1 : AppBase
{
    public AppV1()
    {
        var services = new ServiceCollection();
        services.AddFusion(fusion => {
            fusion.AddComputeService<IProductService, InMemoryProductService>();
            fusion.AddComputeService<ICartService, InMemoryCartService>();
        });
        ClientServices = HostServices = services.BuildServiceProvider();
    }
}
```

One thing is clear now: this is code that adds a magic ingredient 
to a pretty usual dish to give it superpowers. Sorry, can't resist
to depict it in symbols: 🧝=🥣+🦄

Seriously, so how does it work?

`AddComputeService` registers so-called Compute Service - 
a singleton, which proxy type is generated in the runtime, 
but derives from the type you provide, i.e. 
`InMemoryProductService` / `InMemoryCartService` in above case.
The proxy "decorates" every method marked by
`[ComputeMethod]` with a special wrapper:
1. First, it computes the `key` for this call. 
   It is ~ `(serviceInstance, method, arguments.ExceptCancellationToken())`
   tuple.
2. Then it checks if the `IComputed` instance associated with 
   the same key still exists in RAM. 
   `ComputedRegistry` is the underlying type that caches
   weak references to all of `IComputed` instances and 
   helps to find them.
   If `IComputed` instance is found and it's still `Consistent`,
   the wrapper "strips" it by returning its `Value` -
   in other words, it returns the cached answer.
   Note that `Value` may throw an exception - as you 
   might guess, exceptions are cached the same way
   as well, though by default they auto-expire in 1 second.
3. Otherwise it acquires async lock for the `key` and retries
   #2 inside this lock. You probably recognize this is
   just a
   [double-checked locking](https://en.wikipedia.org/wiki/Double-checked_locking).
4. If all of this didn't help to find the cached "answer",
   the base method (i.e. your original one) is called 
   to *compute* it. But before the *computation* part start,
   the `IComputed` instance that's going to store its
   outcome gets temporarily exposed via `Computed.GetCurrent()`
   for the duration of the computation.

   Why? Well, it wasn't mentioned, but once an `IComputed`
   gets "stripped" on step #2, #3, and even later on #4,
   it's also registered as a dependency of any 
   other `IComputed` that's currently exposed via
   `Computed.GetCurrent()`. **And this is how `IComputed`
   instances "learn" all the components they're "built" from.**

   When the computation completes, the newly created
   `IComputed` gets registered in `ComputedRegistry`
   and "stripped" the same way to return its `Value`
   and possibly, become a dependency of another
   `IComputed`.

The gist is: any Compute Service methods marked by 
`[ComputeMethod]` attribute get a special behavior, 
which:
- Caches method call results
- Makes sure that for a given set of arguments 
  just one computation of call result may run 
  concurrently (due to async lock inside)
- Finally, it builds a dependency graph of method
  computation results under the hood, where nodes
  are `IComputed` instances, and edges are stored
  in their `_used` and `_usedBy` fields.

And even though normally you don't see these `IComputed`
instances, there are APIs allowing you to:
- "Pull" the `IComputed` that "backs" certain `[ComputeMethod]` call result
- Invalidate it, i.e. mark it inconsistent
- Await for its invalidation
- Or even get the most up-to-date version of a possibly invalidated
  `IComputed` - either a cached or a newly computed one.

Do you remember the code "watching" for cart changes in the 
beginning?

```cs
// Computed.CaptureAsync pulls the `IComputed` storing the
// result of a call to the first [ComputeMethod] made from 
// the delegate it gets, i.e. the result of
// cartService.GetTotalAsync(cartId, ct) in this case
var computed = await Computed.CaptureAsync(
    ct => cartService.GetTotalAsync(cartId, ct), cancellationToken);
while (true) {
    WriteLine($"  {cartId}: total = {computed.Value}");
    // IComputed.WhenInvalidatedAsync awaits for the invalidation.
    // It returns immediately if 
    // (computed.State == ConsistencyState.Invalidated)
    await computed.WhenInvalidatedAsync(cancellationToken);
    // Finally, this is how you update IComputed instances.
    // As you might notice, they're almost immutable, 
    // so "update" always means creation of a new instance.
    computed = await computed.UpdateAsync(false, cancellationToken);
}
```

Now, how these dependencies get created? Let's look at
`InMemoryCartService.GetTotalAsync` again:
```cs
public virtual async Task<decimal> GetTotalAsync(
    string id, CancellationToken cancellationToken = default)
{
    // Dependency: this.FindAsync(id)!
    var cart = await FindAsync(id, cancellationToken);
    if (cart == null)
        return 0;
    var total = 0M;
    foreach (var (productId, quantity) in cart.Items) {
        // Dependency: _products.FindAsync(productId)!
        var product = await _products.FindAsync(productId, cancellationToken);
        total += (product?.Price ?? 0M) * quantity;
    }
    return total;
}
```

As you see, any result of `GetTotalAsync(id)` becomes
dependent on:
- Cart content - for the cart with this `id`
- Every product that's referenced by items in this cart.

> If you want to learn more about Compute Services and
> `IComputed`, check out [Part 1](Part01.md) and [Part 2](Part02.md)
> of this Tutorial later.

Now it's time to demystify how `FindAsync(productId)` call result gets
invalidated once a product with `productId` gets changed.

Again, remember this code?
```cs
public virtual Task EditAsync(EditCommand<Product> command, CancellationToken cancellationToken = default)
{
    var (productId, product) = command;
    if (string.IsNullOrEmpty(productId))
        throw new ArgumentOutOfRangeException(nameof(command));
    if (Computed.IsInvalidating()) {
        // This is the invalidation block.
        // Every [ComputeMethod] result you "touch" here
        // instantly becomes a 🎃 (gets invalidated)!
        FindAsync(productId, default).Ignore();
        return Task.CompletedTask;
    }

    if (product == null)
        _products.Remove(productId, out _);
    else
        _products[productId] = product;
    return Task.CompletedTask;
}
```

And if you look into similar `EditAsync` for in `InMemoryCartService`, 
you'll find a very similar block there:
```cs
if (Computed.IsInvalidating()) {
    FindAsync(cartId, default).Ignore();
    return Task.CompletedTask;
}
```

So now you have *almost* the full picture:
- Low-level methods (the ones that don't have any 
  dependencies) are invalidated explicitly &ndash;
  by the invalidation blocks in command handlers 
  that may render cached results of calls to such methods 
  inconsistent with the ground truth.
- High-level methods like `GetTotalAsync` are invalidated
  automatically due to invalidation of their dependencies.
  This is why you don't see a call to `GetTotalAsync` in 
  any of invalidation blocks.

What's missing is how it happens that when you call `EditAsync`,
***both** `if (Computed.IsInvalidating()) { ... }` and the code
outside of this block runs, assuming this block contains `return` 
statement?

I'll give a brief answer here:
- Yes, in reality any Compute Service method decorated with 
  `[CommandHandler]` is called `N + 1` times, where `N` is the
  number of servers in your cluster 🙀
- The first call is the normal one - it makes all the changes
- `N` more calls are made inside so-called invalidation scope - i.e. inside
  `using (Computed.Invalidate()) { ... }` block, and they are reliably
  executed on every server in your cluster, including the one 
  where the command was originally executed.
- Moreover, when your command (the `Task<T>` running it) completes
  on the original server, it's guaranteed that both its normal handler 
  call and "the invalidation call" were completed for it locally.

Under the hood all of this is powered by similar AOP-style
decorators and [CommandR](Part09.md) - a [MediatR](https://github.com/jbogard/MediatR) -
style abstraction used by Fusion to implement its command processing
pipeline.

Methods marked with `[CommandHandler]` behave very differently
from methods marked by `[ComputeMethod]` - in fact, there
is nothing common at all.
The wrapper logic for command handlers does nothing but routes 
every call to `ICommander`. This allows you to call such methods
directly - note that if this logic won't exist, calling such a method
directly would be a mistake, because such call won't trigger
the whole command processing pipeline for the used command.

So wrappers for `[CommandHandler]`-s declared in Compute Services
exist to unify this: you are free to invoke such commands by either 
throwing them to `ICommander.CallAsync(command, ...)`,
or just calling them directly. Later you'll learn that this
feature also enables Fusion to implement clients for APIs 
like `ICartService`, and to route client-side commands 
(sent to client-side `ICommander` instances) to these
clients to execute them on server side. 

Ok, but what happens when the command is processed by 
`ICommander`? As in case with MediatR, it means triggering 
command handler pipeline for this type of command.
Fusion injects a number of its own middleware-like handlers for
Compute Service commands. These handlers run your command handler 
(the final one) in the end, but also provide all the infrastructure 
needed to "replay" this command in the invalidation mode on
every host. In particular, they:
- Provide an abstraction allowing to start a transaction
  for this command and get `DbContext`s associated
  with this transaction.
- Log the command to the operation log on commit
- Notify other hosts that operation log was updated
- Replay the command in the invalidation mode locally.

Btw, "replaying the command in the invalidation mode" means:
- Restoring the "operation items". Later I'll show you can
  pass the information from a "normal" command handler "pass"
  to the subsequent "invalidation pass" run. 
  Typically you need this to properly invalidate something
  related to what was deleted during the "normal" pass.
- Running the same command handler, but inside 
  `using (Computed.Invalidate()) { ... }` block.

☝ The pipeline described above is called **"Operations Framework"** 
(**OF** further) - in fact, it's just a set of handlers and services 
for CommandR providing this multi-host invalidation pipeline for every
Fusion's Compute Service.

And a few final remarks on this:
1. The pipeline described above is used very partially in `v1`'s 
   case: there are no other hosts, no database, and thus no calls
   enabling all these integrations were made when the IoC container
   was configured. So only a very core part of this pipeline running
   handlers normally + in the invalidation mode is used.
2. **No, this is not how Fusion delivers changes to every remote
   client** (e.g. Blazor WASM running in your browser). 
   This pipeline is server-side only. 

> If you want to learn all the details about this - check out 
> [Part 8](Part08.md), [Part 9](Part09.md), and [Part 10](Part10.md) 
> of the Tutorial 😎

## Version 2: Switching to EF Core

Code: [src/HelloCart/v2](https://github.com/servicetitan/Stl.Fusion.Samples/tree/master/src/HelloCart/v2)

> ☝ We will move WAY FASTER now - this and every following version
> will require just about 5 minutes of your time.

Here is what code inside `AppV2` constructor does:

```cs
// This is exactly the same Compute Service registration code you saw earlier
services.AddFusion(fusion => {
    fusion.AddComputeService<IProductService, DbProductService>();
    fusion.AddComputeService<ICartService, DbCartService>();
});

// This also a usual way to add a pooled IDbContextFactory -
// a preferable way of accessing DbContexts nowadays
var appTempDir = PathEx.GetApplicationTempDirectory("", true);
var dbPath = appTempDir & "HelloCart_v01.db";
services.AddDbContextFactory<AppDbContext>(b => {
    b.UseSqlite($"Data Source={dbPath}");
    b.EnableSensitiveDataLogging();
});

// AddDbContextServices is just a convenience builder allowing
// to omit DbContext type in misc. normal and extension methods 
// it has
services.AddDbContextServices<AppDbContext>(b => {
    // This call enabled Operations Framework (OF) for AppDbContext. 
    b.AddDbOperations((_, o) => {
        // Here we tell operation log reader that it should fetch the
        // tail of operation log every 5 seconds no matter what.
        o.UnconditionalWakeUpPeriod = TimeSpan.FromSeconds(5);
    });
    // And this call tells that hosts will use a shared file
    // to "message" each other that operation log was updated.
    // In fact, they'll just be "touching" this file once
    // this happens and watch for change of its modify date.
    // You shouldn't use this mechanism in real multi-host
    // scenario, but it works well if you just want to test
    // multi-host invalidation on a single host by running
    // multiple processes there.
    b.AddFileBasedDbOperationLogChangeTracking(dbPath + "_changed");
});
ClientServices = HostServices = services.BuildServiceProvider();
```

`AppV2.InitializeAsync` simply re-created the DB:

```cs
await using var dbContext = HostServices.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.EnsureCreatedAsync();
await base.InitializeAsync();
```

Now, if you look at `DbProductService` and `DbCartService`, you'll notice
just a few differences between them and any regular service that
reads/writes the DB:
1. They are inherited from `DbServiceBase<AppDbContext>`. This type
   is just a convenience helper providing a few protected methods
   for services that are supposed to access the DB.
2. One of these methods is `CreateDbContext` - you may see it's typically
   used like this:
    ```cs
    await using var dbContext = CreateDbContext();
    // ... code using dbContext
    ```
   By default, `CreateDbContext` returns **a read-only `DbContext` with
   change tracking disabled**. 
   As you might guess, this method of getting `DbContext` is supposed 
   to be used in `[ComputeMethod]`-s, i.e. query-style methods that
   aren't supposed to change anything or rely on change tracking.

   "Read-only" means this `DbContext` "throws" on attempt to call 
   `SaveChangesAsync`.
3. And another one is `CreateCommandDbContextAsync`, which is used like this:
    ```cs
    await using var dbContext = await CreateCommandDbContextAsync(cancellationToken);
    // ... code using dbContext
    ```
   Contrary to the previous method, this method is used to create
   `DbContext` inside command handlers, and once it's called,
   it also starts the transaction associated with the current command
   (which is why this method returns `Task<TDbContext>`).
   The transaction is auto-committed once your handler completes normally
   (i.e. w/o an exception), moreover, the operation log entry
   describing the current command will be persisted as part of this 
   transaction.

   As you might guess, the `DbContext` provided by this method is
   **read-write and with enabled change tracking**. Moreover,
   if you call it multiple times, you'll get different `DbContext`-s,
   but all of them will share the same `DbConnection`, and consequently,
   will "see" the DB through the same transaction.

And that's it. So to use Fusion with EF, you must:
- Make a couple extra calls during IoC container configuration
  to enable Operations Framework
- Inherit your Compute Services from `DbServiceBase<TDbContext>`
  and rely on its `CreateDbContext` / `CreateCommandDbContextAsync`
  to get `DbContext`-s. Alternatively, you just see what these
  methods do and use the same code in Compute Services that
  can't be inherited from `DbServiceBase<TDbContext>`.

## Version 3: Switching to production-grade EF Core

Code: [src/HelloCart/v3](https://github.com/servicetitan/Stl.Fusion.Samples/tree/master/src/HelloCart/v3)

The `v2` code is actually already good enough, but one small improvement
can make it way better:

```cs
b.AddDbEntityResolver<string, DbProduct>();
b.AddDbEntityResolver<string, DbCart>((_, options) => {
    // Cart is always loaded together with items
    options.QueryTransformer = carts => carts.Include(c => c.Items);
});
```

This code registers two entity resolvers - one for `DbProduct` type,
and another one - for `DbCart` type (`string` is the type of key of 
these entities). 

Entity resolvers are helpers grouping multiple requests to 
find the entity by its key together and resolving all of them by
sending a single DB query.

The pseudo-code of the "main loop" of every entity resolver looks ~
as follows:
```cs
while (NotDisposed()) {
    var (tasks, keys) = GetNewEntityResolutionRequestTasks();
    // GetEntitiesAsync sends a single DB query with "{key} in (...)" clause
    var entities = await GetEntitiesAsync(keys); ..)
    CompleteEntityResolutionTasks(tasks, keys, entities);
}
```

Here is an example of how to use such resolvers:
```cs
public virtual async Task<Product?> FindAsync(string id, CancellationToken cancellationToken = default)
{
    var dbProduct = await _productResolver.TryGetAsync(id, cancellationToken);
    if (dbProduct == null)
        return null;
    return new Product() { Id = dbProduct.Id, Price = dbProduct.Price };
}
```

Guess why this important? Look at the production-grade `GetTotalAsync` code:
```cs
public virtual async Task<decimal> GetTotalAsync(string id, CancellationToken cancellationToken = default)
{
    var cart = await FindAsync(id, cancellationToken);
    if (cart == null)
        return 0;
    var itemTotals = await Task.WhenAll(cart.Items.Select(async item => {
        var product = await _products.FindAsync(item.Key, cancellationToken);
        return item.Value * (product?.Price ?? 0M);
    }));
    return itemTotals.Sum();
}
```

Contrary to the previous version, it fetches all the products **in parallel**.
But why?
- First, it "hopes" that Fusion will resolve most of `FindAsync` calls via
  its `IComputed` instance cache, i.e. without hitting the DB. And why 
  shouldn't it try to do this in parallel, if this possible?
- But now imagine the case when Fusion's cache is empty, or it doesn't
  store the result of `FindAsync` for every product that's in the cart.
  This is where entity resolver used in `FindAsync` kicks in: most
  likely it will run just 1 or 2 queries to resolve every remaining
  product that `GetTotalAsync` throws - and moreover, since all
  these calls "flow" through `FindAsync`, these results will be 
  cached in Fusion's cache too!

So crafting highly efficient Compute Services based on EF Core is actually 
quite easy - if you think what's the extra code you have to write, 
you'll find it's mainly `if (Computed.IsInvalidating()) { ... }` blocks -
the rest is something you'd likely have otherwise at some point as well!

And if you're curious how much of this "extra" a real app is expected to 
have - check out [Board Games](https://github.com/alexyakunin/BoardGames).
It's mentioned in its 
[README.md](https://github.com/alexyakunin/BoardGames/blob/main/README.md)
that this whole app has 
[just about 35 extra lines of code](https://github.com/alexyakunin/BoardGames/search?q=IsInvalidating) 
responsible for the invalidation!
In other words, **Fusion brought the cost of all real-time features this app 
has to nearly zero there**.


// Work in progress - to be continued.
