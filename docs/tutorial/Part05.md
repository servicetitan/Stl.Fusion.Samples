# Part 5: Caching and Fusion on Server-Side Only

Even though Fusion supports RPC, you can use it on server-side only,
and performance is the main benefit of this. Below is the output of
[Caching Sample](https://github.com/servicetitan/Stl.Fusion.Samples/tree/master/src/Caching):

```text
Local services:
Fusion's Compute Service [-> EF Core -> SQL Server]:
  Reads         : 27.55M operations/s
Regular Service [-> EF Core -> SQL Server]:
  Reads         : 25.05K operations/s

Remote services:
Fusion's Replica Client [-> HTTP+WebSocket -> ASP.NET Core -> Compute Service -> EF Core -> SQL Server]:
  Reads         : 20.29M operations/s
RestEase Client [-> HTTP -> ASP.NET Core -> Compute Service -> EF Core -> SQL Server]:
  Reads         : 127.96K operations/s
RestEase Client [-> HTTP -> ASP.NET Core -> Regular Service -> EF Core -> SQL Server]:
  Reads         : 20.46K operations/s
```

Last two results are the most interesting in the context of this part:

- A tiny EF Core-based service exposed via ASP.NET Core controller
  serves **20,500** requests per second. That's already a lot -
  mostly, because its data set fully fits in RAM on SQL Server.
- An identical service relying on Fusion (it's literally the same code
  plus Fusion's `[ComputeMethod]` and `Computed.Invalidate` calls)
  boosts this number to **128,000** requests per second.

And that's the main reason to use Fusion on server-side only:
5-10x performance boost with a relatively tiny amount of changes.
[Similarly to incremental builds](https://alexyakunin.medium.com/the-ungreen-web-why-our-web-apps-are-terribly-inefficient-28791ed48035?source=friends_link&sk=74fb46086ca13ff4fea387d6245cb52b),
the more complex your logic is, the more you are expected to gain.

## The Fundamentals

You already know that `IComputed<T>` instances are reused, but so far
we didn't talk much about the details. Let's learn some specific
aspects of this behavior before jumping to caching.

The service below prints a message once its `GetAsync` method
is actually computed (i.e. its cached value for a given argument isn't reused)
and returns the same value as its input. We'll be using it to
find out when `IComputed` instances are actually reused.

``` cs --editable false --region Part05_Service1 --source-file Part05.cs
[ComputeService] // You don't need this attribute if you manually register such services
public class Service1
{
    [ComputeMethod]
    public virtual async Task<string> GetAsync(string key)
    {
        WriteLine($"{nameof(GetAsync)}({key})");
        return key;
    }
}

public static IServiceProvider CreateServices()
{
    var services = new ServiceCollection();
    services.AddFusion();
    services.AttributeBased().AddServicesFrom(Assembly.GetExecutingAssembly());
    return services.BuildServiceProvider();
}
```

First, `IComputed` instances aren't "cached" by default - they're just
reused while it's possible:

``` cs --region Part05_Caching1 --source-file Part05.cs
var service = CreateServices().GetService<Service1>();
// var computed = await Computed.CaptureAsync(_ => counters.GetAsync("a"));
WriteLine(await service.GetAsync("a"));
WriteLine(await service.GetAsync("a"));
GC.Collect();
WriteLine("GC.Collect()");
WriteLine(await service.GetAsync("a"));
WriteLine(await service.GetAsync("a"));
```

The output:

```text
GetAsync(a)
a
a
GC.Collect()
GetAsync(a)
a
a
```

As you see, `GC.Collect()` call removes cached `IComputed`
for `GetAsync("a")` - and that's why `GetAsync(a)` is printed
twice here.

All of this means that most likely Fusion holds a
[weak reference](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/weak-references)
to this value (in reality it uses `GCHandle`-s for performance reasons, but
technically they do the same).

Let's prove this by uncomment the commented line:

``` cs --region Part05_Caching2 --source-file Part05.cs
var service = CreateServices().GetService<Service1>();
var computed = await Computed.CaptureAsync(_ => service.GetAsync("a"));
WriteLine(await service.GetAsync("a"));
WriteLine(await service.GetAsync("a"));
GC.Collect();
WriteLine("GC.Collect()");
WriteLine(await service.GetAsync("a"));
WriteLine(await service.GetAsync("a"));
```

The output:

```text
GetAsync(a)
a
a
GC.Collect()
a
a
```

As you see, assigning a strong reference to `IComputed` is enough
to ensure it won't recompute on the next call.

> So to truly cache some `IComputed`, you need to store a strong
> reference to it and hold it while you want it to be cached.

Now, if you compute `f(x)`, is it enough to store
a computed for its output to ensure its dependencies
are cached too? Let's test this:

``` cs --editable false --region Part05_Service2 --source-file Part05.cs
[ComputeService]
public class Service2
{
    [ComputeMethod]
    public virtual async Task<string> GetAsync(string key)
    {
        WriteLine($"{nameof(GetAsync)}({key})");
        return key;
    }

    [ComputeMethod]
    public virtual async Task<string> CombineAsync(string key1, string key2)
    {
        WriteLine($"{nameof(CombineAsync)}({key1}, {key2})");
        return await GetAsync(key1) + await GetAsync(key2);
    }
}
```

``` cs --region Part05_Caching3 --source-file Part05.cs
var service = CreateServices().GetService<Service2>();
var computed = await Computed.CaptureAsync(_ => service.CombineAsync("a", "b"));
WriteLine("computed = CombineAsync(a, b) completed");
WriteLine(await service.CombineAsync("a", "b"));
WriteLine(await service.GetAsync("a"));
WriteLine(await service.GetAsync("b"));
WriteLine(await service.CombineAsync("a", "c"));
GC.Collect();
WriteLine("GC.Collect() completed");
WriteLine(await service.GetAsync("a"));
WriteLine(await service.GetAsync("b"));
WriteLine(await service.CombineAsync("a", "c"));
```

The output:

```text
CombineAsync(a, b)
GetAsync(a)
GetAsync(b)
computed = CombineAsync(a, b) completed
ab
a
b
CombineAsync(a, c)
GetAsync(c)
ac
GC.Collect() completed
a
b
CombineAsync(a, c)
GetAsync(c)
ac
```

As you see, yes,

> Strong referencing an `IComputed` ensures every other `IComputed`
> instance it depends on also stays in memory.

Let's check if the opposite is true as well:

``` cs --region Part05_Caching4 --source-file Part05.cs
var service = CreateServices().GetService<Service2>();
var computed = await Computed.CaptureAsync(_ => service.GetAsync("a"));
WriteLine("computed = GetAsync(a) completed");
WriteLine(await service.CombineAsync("a", "b"));
GC.Collect();
WriteLine("GC.Collect() completed");
WriteLine(await service.CombineAsync("a", "b"));
```

The output:

```text
GetAsync(a)
computed = GetAsync(a) completed
CombineAsync(a, b)
GetAsync(b)
ab
GC.Collect() completed
CombineAsync(a, b)
GetAsync(b)
ab
```

So the opposite is not true.

But why Fusion behaves this way? The answer is actually super simple:

* Fusion has to strong-reference every computed instance used to produce an output
  because if any of them gets garbage collected before the output itself,
  the invalidation passing through such an instance simply won't reach the output.
* But since it has to propagate the invalidation events from dependencies
  (used values) to dependants (values produced from the used ones),
  it also has to reference the direct dependants from every dependency.
  And these references have to be either weak
  or simply shouldn't be .NET object references, coz if they were strong
  references, this would almost certainly keep the whole graph of `IComputed`
  in memory, which is highly undesirable. That's why Fusion uses the second
  option here - it stores keys of direct dependants of every `IComputed`
  and resolves them via the same registry as it uses to resolve `IComputed`
  instances for method calls.
* Interestingly, there is a fundamental reason to weak reference every
  `IComputed`: imagine Fusion doesn't do this, so calling the same compute
  method twice with the same arguments may produce two different `IComputed`
  instances representing the output (of the same computation). Now imagine
  you invalidate the first one (and thus all of its dependants), but
  don't do anything with the second one (so its dependants stay valid).
  As you see, it's a partial invalidation, i.e. something that may cause
  a long-term inconsistency - which is why Fusion ensures that at any
  moment of time there can be only one `IComptuted` instance describing
  given computation.

**Default caching behavior - a summary:**

* Nothing is really cached, but everything is weak-referenced
* When you compute a new value, every value used to produce it
  becomes strong-referenced from the produced one
* So if you strong reference some `IComputed`, you effectively
  hold the whole graph of what it's composed from in memory.
* If `IComputed` containing the output of `f(someArgs)` doesn't
  exist in RAM, it also means none of its possible dependants
  exist in RAM.

## Caching Options

As you probably already understood, Fusion allows you to
implement any desirable caching behavior: all you need
is your own service that will hold a strong reference
to whatever you want to cache for as long as you want.

Besides that, Fusion offers two built-in options:

* `[ComputeMethod(KeepAliveTime = TimeInSeconds)]`,
  ensures a strong reference w/ specified expiration
  time is added to
  [Timeouts.KeepAlive](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion/Internal/Timeouts.cs)
  timer set every time the output is used. In fact, it sets a minimum
  time the output of a function stays in RAM.
* `[Swap(SwapTime = TimeInSeconds)]` enables swapping feature
  for the compute method's output. It's a kind of similar to
  OS page swapping, but stores & loads back the output instead.

Let's look at how they work.

### ComputeMethodAttribute.KeepAliveTime

Let's just add `KeepAliveTime` to the service we were using previously:

``` cs --editable false --region Part05_Service3 --source-file Part05.cs
[ComputeService]
public class Service3
{
    [ComputeMethod]
    public virtual async Task<string> GetAsync(string key)
    {
        WriteLine($"{nameof(GetAsync)}({key})");
        return key;
    }

    [ComputeMethod(KeepAliveTime = 0.3)] // KeepAliveTime was added
    public virtual async Task<string> CombineAsync(string key1, string key2)
    {
        WriteLine($"{nameof(CombineAsync)}({key1}, {key2})");
        return await GetAsync(key1) + await GetAsync(key2);
    }
}
```

And run this code:

``` cs --region Part05_Caching5 --source-file Part05.cs
var service = CreateServices().GetService<Service3>();
WriteLine(await service.CombineAsync("a", "b"));
WriteLine(await service.GetAsync("a"));
WriteLine(await service.GetAsync("x"));
GC.Collect();
WriteLine("GC.Collect()");
WriteLine(await service.CombineAsync("a", "b"));
WriteLine(await service.GetAsync("a"));
WriteLine(await service.GetAsync("x"));
await Task.Delay(1000);
GC.Collect();
WriteLine("Task.Delay(...) and GC.Collect()");
WriteLine(await service.CombineAsync("a", "b"));
WriteLine(await service.GetAsync("a"));
WriteLine(await service.GetAsync("x"));
```

The output:

```text
CombineAsync(a, b)
GetAsync(a)
GetAsync(b)
ab
a
GetAsync(x)
x
GC.Collect()
ab
a
GetAsync(x)
x
Task.Delay(...) and GC.Collect()
CombineAsync(a, b)
GetAsync(a)
GetAsync(b)
ab
a
GetAsync(x)
x
```

As you see, `KeepAliveTime` does exactly what's expected:

- It holds a strong reference to the output of `CombineAsync` for 0.3 seconds,
  so the output of `CombineAsync("a", "b")` gets cached for 0.3s
- Since `CombineAsync` calls `GetAsync`, the outputs of
  `GetAsync("a")` and `GetAsync("b")` are cached for 0.3s too
- But not the output of `GetAsync("x")`, which wasn't used in any of
  `CombineAsync` calls in this example.

That's basically it on `KeepAliveTime`.

A few tips on how to use it:

* You should apply it mainly to the final outputs - i.e. compute
  methods that are either exposed via API or used in your UI.
* Applying it to other compute methods is fine too, though keep in mind
  that whatever is used by top level methods with `KeepAliveTime`
  is anyway cached for the same period, so probably you don't need this.
* And in general, keep in mind that ideally you want to "recompose"
  or aggregate the outputs of compute methods rather than "rewrite".
  In other words, if you have a chance to use the same object you get
  from the downstream method - do this, because this won't incur
  use of extra RAM.
* This is also why you might want to return just immutable objects from
  compute methods &ndash; and C# 9 records come quite handy here.
  Alternatively, you can use
  [freezable pattern](https://github.com/jbe2277/waf/wiki/Freezable-Pattern)
  implementation from
  [Stl.Frozen](https://github.com/servicetitan/Stl.Fusion/tree/master/src/Stl/Frozen);
  actually, you can use any implementation, though if Fusion sees you return
  `Stl.Frozen.IFrozen` from compute method, it automatically freezes the output.

### [Swap] attribute

Let's jump straight to the example:

``` cs --editable false --region Part05_Service4 --source-file Part05.cs
[ComputeService]
public class Service4
{
    [ComputeMethod(KeepAliveTime = 1), Swap(0.1)]
    public virtual async Task<string> GetAsync(string key)
    {
        WriteLine($"{nameof(GetAsync)}({key})");
        return key;
    }
}

[Service(typeof(ISwapService))]
public class DemoSwapService : SimpleSwapService
{
    protected override ValueTask StoreAsync(string key, string value, CancellationToken cancellationToken)
    {
        WriteLine($"Swap: {key} <- {value}");
        return base.StoreAsync(key, value, cancellationToken);
    }

    protected override ValueTask<bool> RenewAsync(string key, CancellationToken cancellationToken)
    {
        WriteLine($"Swap: {key} <- [try renew]");
        return base.RenewAsync(key, cancellationToken);
    }

    protected override async ValueTask<Option<string>> LoadAsync(string key, CancellationToken cancellationToken)
    {
        var result = await base.LoadAsync(key, cancellationToken);
        WriteLine($"Swap: {key} -> {result}");
        return result;
    }
}
```

``` cs --region Part05_Caching6 --source-file Part05.cs
var service = CreateServices().GetService<Service4>();
WriteLine(await service.GetAsync("a"));
await Task.Delay(500);
GC.Collect();
WriteLine("Task.Delay(500) and GC.Collect()");
WriteLine(await service.GetAsync("a"));
await Task.Delay(1500);
GC.Collect();
WriteLine("Task.Delay(1500) and GC.Collect()");
WriteLine(await service.GetAsync("a"));
```

The output:

```text
GetAsync(a)
a
Swap: Castle.Proxies.Service4Proxy|@26|a <- [try renew]
Swap: Castle.Proxies.Service4Proxy|@26|a <- {"$type":"Stl.ResultBox`1[[System.String, System.Private.CoreLib]], Stl","UnsafeValue":"a"}
Task.Delay(500) and GC.Collect()
Swap: Castle.Proxies.Service4Proxy|@26|a -> Some({"$type":"Stl.ResultBox`1[[System.String, System.Private.CoreLib]], Stl","UnsafeValue":"a"})
a
Swap: Castle.Proxies.Service4Proxy|@26|a <- [try renew]
Task.Delay(1500) and GC.Collect()
GetAsync(a)
a
```

So what's going on here?

* `[ComputeMethod(KeepAliveTime = 1)]` tells the `IComputed` describing the output
  of `GetAsync` should stay in RAM for 1s
* `[Swap(0.1)]` tells its value should be swapped out once 0.1s pass after
  the last attempt to use it, which, in turn, means that if someone tries
  to access it after it was swapped, Fusion will try to load it back, and
  it might take some time.
* Our `DemoSwapService` is inherited from `SimpleSwapService`,
  which stores keys and values in internal `ConcurrentDictionary<string, string>`.

And as you see in the output, that's exactly what's going on.
Maybe just the last part of it looks weird - i.e. it seems
that once keep alive time passes, the value, even though it was
swapped out, becomes unusable - why?

Wait... Why do we need both `KeepAliveTime` and `[Swap]`?
Why there is `[Swap]` at the first place? Why Fusion can't
simply store every `[IComputed]` in external cache?

If you read everything till this point, you probably already know
the answers:

* Storing the value of `IComputed` externally isn't enough -
  you also need to store refs to dependencies and dependants,
  otherwise the invalidation won't work properly.
  And even though the value might stay the same, the set of dependants
  may change over time - moreover, these changes could be
  nearly as frequent as reads of this value!
* In addition, the external cache you use must ensure that while
  some dependant stays in cache, all of its dependencies stay there too.
  And how many cache implementations do support this?

In other words, storing the graph of dependencies in external cache
seems quite inefficient due to frequent updates and the requirement
to keep dependencies alive while their dependants are alive.

What's totally reasonable though is to cache just values - obviously,
assuming these values are expected to be large enough.
And that's exactly what swapping does. All you need is to implement
a service that actually does this. Sorry, but for now there is
no built-in implementations for popular caches, but you are welcome
to contribute, and inheriting from `SimpleSwapService<string>` or
`SwapServiceBase` could help you a lot to add it.

You probably understand now that `KeepAliveTime` should be
higher (typically - much higher) than `SwapTime`:
once `KeepAliveTime` passes, you may loose the `IComputed` instance.
And once this happens, its "swapped out" value becomes unusable too,
because its dependency graph is destroyed, and thus there
is no way to tell if it's consistent or not now without recomputing it
(and building a new dependency graph).

This explains why the output shows the value was recomputed after 1500ms delay,
even though it wasn't recomputed after 500ms delay.

**P.S.** If you love algorithms and data structures, check out
[ConcurrentTimerSet<TTimer>](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl/Time/ConcurrentTimerSet.cs) -
Fusion uses its own implementation of timers to ensure they
scale much better than `Task.Delay` (which relies on
[TimerQueue](https://referencesource.microsoft.com/#mscorlib/system/threading/timer.cs,29)),
though this comes at cost of fire precision: Fusion timers fire only
[4 times per second](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Fusion/Internal/Timeouts.cs#L20).
Under the hood, `ConcurrentTimerSet` uses
[RadixHeapSet](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl/Collections/RadixHeapSet.cs) -
basically, a [Radix Heap](http://ssp.impulsetrain.com/radix-heap.html)
supporting `O(1)` find and delete operations.

#### [Next: Part 6 &raquo;](./Part06.md) | [Tutorial Home](./README.md)

