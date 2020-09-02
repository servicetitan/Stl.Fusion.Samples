# Part 2: Computed Values and IComputed&lt;T&gt;

We're going to use the same `CounterService` and `CreateServices` helper
as in Part 1:

``` cs --editable false --region Part02_CounterService --source-file Part02.cs
[ComputeService] // You don't need this attribute if you manually register such services
public class CounterService
{
    private readonly ConcurrentDictionary<string, int> _counters = new ConcurrentDictionary<string, int>();

    [ComputeMethod]
    public virtual async Task<int> GetAsync(string key)
    {
        WriteLine($"{nameof(GetAsync)}({key})");
        return _counters.TryGetValue(key, out var value) ? value : 0;
    }

    public void Increment(string key)
    {
        WriteLine($"{nameof(Increment)}({key})");
        _counters.AddOrUpdate(key, k => 1, (k, v) => v + 1);
        Computed.Invalidate(() => GetAsync(key));
    }
}

public static IServiceProvider CreateServices()
    => new ServiceCollection()
        .AddFusionCore()
        .AddDiscoveredServices(Assembly.GetExecutingAssembly())
        .BuildServiceProvider();
```

First, let's try to "pull" `IComputed<T>` instance created behind the
scenes for a given call:

``` cs --region Part02_CaptureComputed --source-file Part02.cs
var counters = CreateServices().GetService<CounterService>();
var computed = await Computed.CaptureAsync(_ => counters.GetAsync("a"));
WriteLine($"Computed: {computed}");
WriteLine($"- IsConsistent(): {computed.IsConsistent()}");
WriteLine($"- Value:          {computed.Value}");
```

The output:

```text
GetAsync(a)
Computed: Computed`1(Intercepted:CounterService.GetAsync(a) @xIs0saqEU, State: Consistent)
- IsConsistent(): True
- Value:          0
```

As you may notice, `IComputed<T>` stores:

- Some representation of its input: `Intercepted:CounterService.GetAsync(a`
- Version: `@xIs0saqEU`
- State: `Consistent`
- Value: `0`

Overall, its key properties include:

* `ConsistencyState`, which transitions from
  `Computing` to `Computed` and `Invalidated` over its lifetime.
  `IsConsistent()` extension method is a shortcut checking whether the state
  is exactly `Consistent`.
  You may find more of such shortcuts by Ctrl-clicking on `IsConsistent()`.
* `Version` property - a unique value for any `IComputed<T>` instance
  in each process. `LTag` struct uses 64-bit integer under the hood,
  so "unique" actually means "unique with very high probability".
* `Output`, `Value` and `Error` - the properties describing the 
  result of the computation.
* `Invalidated` - an event raised on invalidation. Handlers of this event
  should never throw exceptions.

`IComputed<T>` implements a set of interfaces - most notably,

* `IResult<T>` - interestingly, it both "mimics" `IResult<T>` behavior,
  but also exposes a property of `Result<T>` type.
  * `IResult<T>` describes an object that stores the result of computation
    of type `T`, which is either a `Value` of `T`, or an `Error`
    (of `Exception` type). The interface itself provides a number of
    convenience methods (such as `ValueOr(...)`, `IsValue(out var value)`, etc.),
    and there are a few extension methods for it as well.
  * `Result<T>` is its struct-based implementation that's frequently used
    to store the actual result. `IComputed<T>.Output` is the property
    of exactly this type. But since `IComputed<T>` implements `IResult<T>`
    as well (by, basically, forwarding all the calls to its `Output` property),
    you can write `computed.Value` instead of `computed.Output.Value` and so on.
  * Later you'll find out there are other types in `Stl.Fusion` that follow
    the same pattern (i.e. similarly implement `IResult<T>`) - in particular,
    `IState<T>`.
* `IComputedImpl` - an interface  allowing computed instances
  to list themselves as dependencies of other computed instances.
  Most likely you won't ever need to use it, which is why the interface 
  is declared in `Stl.Fusion.Internal` namespace and implemented explicitly.
  It's mentioned here mostly to explain that dependency graph in Fusion
  is explicit, and this interface provides a way to update it. Most of
  other frameworks rely on event handlers to implement cascading invalidations,
  which actually is quite inefficient from GC perspective: it's enough
  to keep reference to *either* dependency or dependent instance to ensure
  *both* stay in RAM. Fusion, on contrary, doesn't prevent unreferenced 
  dependent instances to be garbage collected.
* `IComputed`, `IResult` - "untyped" versions of `IComputed<T>` and
  `IResult<T>`. Similarly to `IEnumerable` (vs `IEnumerable<T>`), you can
  use them when the type of result isn't known.

And finally, there are a few important methods:

* `Invalidate()` - triggers invalidation, which turns the instance 
  into `Invalidated` state. This is the only change that may happen
  with `IComputed<T>` over its lifetime; other than that, computed
  instances are immutable.
  As with `IDisposable.Dispose`, you are free to call this method 
  multiple times, though only the first call matters.
* `WhenInvalidatedAsync(...)` - an extension method allowing to await
  for invalidation.
* `UpdateAsync(...)` - *finds or computes* the consistent version
  of this computed instance. *Finds* means the computation will happen
  if and only if there is no cached consistent instance in
  `ComputedRegistry` (~ a cache tracking the most up-to-date version of
  every computed while they're used).
  Note that it returns the most up-to-date computed, which is *likely,
  but not necessarily, consistent*, because it could be invalidated
  either in between the computation and the moment you check its status,
  or even right during the computation.
* `UseAsync(...)` - a shortcut for `UpdateAsync(true).Value`.
  Gets the most up-to-date value of the current computed and
  makes sure that if this happens inside the computation of another
  computed value, the current `IComputed<T>` (more precisely, its
  most recent version) gets listed as a dependency of this "outer"
  computed.

A diagram to help remembering all of this:

[<img src="./img/IComputed-Class.jpg" width="300"/>](./img/IComputed-Class.jpg)

And a diagram showing how `ConsistencyState` transition works:

[<img src="./img/ConsistencyState.jpg" width="300"/>](./img/ConsistencyState.jpg)

Ok, let's get back to code and see how invalidation *really* works:

``` cs --region Part02_InvalidateComputed1 --source-file Part02.cs
```

The output:
``` text
GetAsync(a)
computed: Computed`1(Intercepted:CounterService.GetAsync(a) @1EhL08uaNN, State: Consistent)
computed.Invalidate()
computed: Computed`1(Intercepted:CounterService.GetAsync(a) @1EhL08uaNN, State: Invalidated)
GetAsync(a)
newComputed: Computed`1(Intercepted:CounterService.GetAsync(a) @1EhL08uaPR, State: Consistent)
```

Compare above code with this example:

``` cs --region Part02_InvalidateComputed2 --source-file Part02.cs
```

The output:
``` text
GetAsync(a)
computed: Computed`1(Intercepted:CounterService.GetAsync(a) @R0oNKnVbo, State: Consistent)
Computed.Invalidate(() => counters.GetAsync("a"))
computed: Computed`1(Intercepted:CounterService.GetAsync(a) @R0oNKnVbo, State: Invalidated)
GetAsync(a)
newComputed: Computed`1(Intercepted:CounterService.GetAsync(a) @R0oNKnVds, State: Consistent)
```

The output is ~ identical. As you might guess,
* `Computed.Invalidate(...)` uses `Computed.Capture` under the hood
  to capture the computed instance and invalidate it.
* `IComputed.UpdateAsync(...)` invokes the method that was used to
  produce it and similarly captures the newly produced computed.

And finally, let's see how you can "observe" the invalidation to 
trigger the update:

``` cs --region Part02_IncrementCounter --source-file Part02.cs
```

The output:
``` text
GetAsync(a)
9/1/2020 5:08:54 PM: 0
Increment(a)
GetAsync(a)
9/1/2020 5:08:55 PM: 1
Increment(a)
GetAsync(a)
9/1/2020 5:08:56 PM: 2
Increment(a)
GetAsync(a)
9/1/2020 5:08:57 PM: 3
Increment(a)
GetAsync(a)
9/1/2020 5:08:58 PM: 4
Increment(a)
GetAsync(a)
9/1/2020 5:08:59 PM: 5
```

So even though Fusion doesn't update anything automatically,
achieving exactly the same behavior with it is pretty straightforward.

A good question to ask is: but why it doesn't, if literally everyone else 
does? E.g. 
[MobX even tries to re-compute all the derivations atomically](https://mobx.js.org/intro/concepts.html) - so why Fusion does literally the opposite?

The answer is:
* If your model is tiny (KO / MobX case), these computations are relatively cheap,
  and moreover, they run on the client. So normally it's fine to run them
  even if you aren't going to use the result.
* On contrary, Fusion is designed to deal with huge models "covering" your 
  whole data; and even though it works on the client too, the most significant
  work it typically does happens on server, where most of its "compute services"
  live. Recomputing every dependency on change isn't just inefficient, but 
  almost never possible in this scenario. 
  
What's totally possible though is to notify the code using certain computed value 
that it became obsolete and thus has to be recomputed. And this notification -
plus maybe some other "knowns" about the domain would let the code to 
determine the right strategy for triggering the recomputation.

Think of these two cases:
- We're recomputing the "view" of a Twitter post (tweet)
- And in one case we know the current user just pressed "Like" icon there,
  so most likely it was invalidated due to this action. In this case
  we can immediately update it to reflect the result of user action;
  there are no potential scalability problems here.
- In another case the post was invalidated w/o current user action. 
  And in this case we may delay the update for several seconds - in fact,
  as for long as we think is reasonable taking into account such factors
  as current backend load or the current rate of actions on this post.

Such an approach allows you to have nearly instant updates in most cases, 
but selectively (i.e. per piece of content + user) throttle the update
rate (without affecting user-induced updates) in cases when instant updates 
create performance problems.

It worth mentioning that Fusion offers all the abstractions you need to
have this behavior, and moreover, similarly to almost-invisible `IComputed<T>`,
you normally don't even need to know these abstractions exist. 
But they'll be ready to help you once you conclude you need throttling.
  
#### [Next: Part 3 &raquo;](./Part02.md) | [Tutorial Home](./README.md)

