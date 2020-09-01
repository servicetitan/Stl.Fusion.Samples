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

As you may notice, `IComputed<T>` has:

- Some representation of its input: `Intercepted:CounterService.GetAsync(a`
- Version: `@xIs0saqEU`
- State: `Consistent`
- Value: `0`

Overall, its important properties and methods include:

* `ConsistencyState` property, which transitions from
  `Computing` to `Computed` and `Invalidated` over its lifetime.
  `IsConsistent()` extension method is a shortcut checking whether the state
  is exactly `Consistent`.
  You may find more of such shortcuts by Ctrl-clicking on `IsConsistent()`.
* `Version` property - a unique value for any `IComputed<T>` instance
  in each process. `LTag` struct uses 64-bit integer under the hood,
  so "unique" actually means "unique with very high probability".
* `Invalidate()` method - turns computed into `Invalidated` state.
  As with `IDisposable.Dispose`, you can call it multiple times,
  though only the first call matters.
* `Invalidated` event - raised on invalidation. Handlers of this event
  should never throw exceptions. Invalidation is *always cascading*.

`IComputed<T>` implements a few interfaces - most notably,

* `IResult<T>` - and interestingly, it both "mimics" `IResult<T>` behavior,
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
  Normally you shouldn't use it, which is why the interface is declared in
  `Stl.Fusion.Internal` namespace and implemented explicitly.

And finally, there are a few asynchronous methods. The most important
ones are:

* `WhenInvalidatedAsync(...)` - an extension method allowing to await
  for invalidation of this instance.
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

#### [Next: Part 3 &raquo;](./Part02.md) | [Tutorial Home](./README.md)

