# HelloCart Sample Overview

HelloCart shows how to add more and more Fusion features to 
gradually make the same simple API production-ready.

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

// Work in progress - to be continued :)

[Program.cs](https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/src/HelloCart/Program.cs)

