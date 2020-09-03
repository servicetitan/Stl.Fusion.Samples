# Part 3: State: IState&lt;T&gt; and its flavors

Video covering this part: **TBD**.

You already know two key concepts of Fusion:

1. **Compute services** allow to write functions that compute everything just
   once and keep the result cached till the moment it either stopped being used,
   or one of its dependencies (a similar output) gets invalidated.
2. `IComputed<T>` &ndash; an abstraction that's actually responsible for
   tracking all these dependencies.

The last missing piece of a puzzle is `IState<T>`, or simply speaking,
a "state". If you used Knockout.js or MobX earlier, state would correspond
to their versions of computed values - i.e. in fact, state simply tracks
the most recent version of some `IComputed<T>`.

Ok, probably it's not quite clear what I just said, but remember that:

- Computed values are immutable - once created in `Computing` state,
  they turn `Consistent` first, but eventually become `Inconsistent`.
- The rules of the game set in such a way that, if we ignore `Computing`
  state, there can be only one `Consistent` version of a computed value corresponding to the same computation at any given moment.
  Every other version (in fact, the older one) will be `Inconsistent`.

So `IState<T>` is what "tracks" the most up-to-date version. There are a few flavors of it behaving slightly differently:

- `IMutableState<T>` is, in fact, a variable that got `IComputed<T>` envelope.
  It's `Computed` property returns always-consistent computed, which gets
  replaced once the `IMutableState.Value` (or `Error`, etc.) is set;
  the old computed gets invalidated.
  If you have such a state, you can use it in one of compute methods
  to make its output dependent on it, or similarly use it in other
  computed state. But describing client-side state of UI components (e.g.
  a value entered into a search box) is its most frequent use case.
- `IComputedState<T>` is ~ the opposite of mutable state, i.e. a state
  that's computed rather than set externally. There is an abstract base
  class corresponding to this interface, but all non-abstract
  implementations of it also implement...
- `ILiveState<T>` - a computed state that triggers its own recomputation
  (update) after the invalidation. And if you think what are the "levers"
  it might have, you'll quickly conclude the only option it needs to control
  is a delay between the invalidation and the update. And that's exactly
  what it offers - its `UpdateDelayer` property references `IUpdateDelayer`,
  which implements the delay. Moreover, any `IUpdateDelayer` also supports
  cancellation of any active delays.
- Finally, there is `ILiveState<T, TLocals>` - a convenience interface
  inheriting from `ILiveState<T>`, but also having `Locals` property of
  `IMutableState<TLocals>` type. In other words, that's a live state
  that's always "dependent" on its `Locals` variable, so once it changes,
  the live state gets invalidated (and by default, gets updated immediately,
  if it's due to `Locals` update).

Let's summarize all of this in a single table:

![](./img/IState-Features.jpg)

And finally, states have a few extra properties:

- Similarly to `IEnumerable<T>` \ `IEnumerable`, there are typed
  and untyped versions of any `IState` interface.
- Similarly to `IComputed<T>`, any state implements `IResult<T>`
  by forwarding all the calls to its `Computed` property.
- `IMutableState<T>` also implements `IMutableResult<T>`
- Any state has `Snapshot` property of `IStateSnapshot<T>` type.
  This property is updated atomically and returns an immutable object describing the current "state" of the `IState<T>`. If you'll
  ever need a "consistent" view of the state, `Snapshot` is
  the way to get it. A good example of where you'd need it would be
  this one:
  - You read `state.HasValue` first, it returns `true`
  - But a subsequent attempt to read `state.Value` fails because
    the state was updated right in between these two reads.
  - Doing the same via `Snapshot.Computed` property ensures
    this can't happen.
- Both `IState<T>` and `IStateSnapshot<T>` expose
  `LastValue` and `LastValueComputed` properties - they
  allow to access the last valid `Value` and its `IComputed<T>`
  exposed by the state. In other words, when state exposes
  an `Error`, `LastValue` still exposes the previous `Value`.
  This feature is quite handy when you need to access both
  the last "correct" value (to e.g. bind it to the UI)
  and the newly observed `Error` (to display it separately).

The diagram describing all of this:

![](./img/IState-Classes.jpg)

> Note that `<T>` absents on the diagram due to limitations of
> [Mermaid.js](https://mermaid-js.github.io/mermaid/#/),
> even though in reality all these interfaces have this parameter.

## Constructing States

There are two ways of doing this:

1. Using `IStateFactory`. Any `IServiceProvider` configured to use
   Fusion (with `.AddFusionCore()`) should resolve it.
2. Subclassing `MutableState<T>`, `LiveState<T>`, etc.
   and (optionally) registering a new type as a service 
   via `.AddState(...)` method.

Normally you need just the first option, and the remaining part of 
this document sticks to it.

## Mutable State

Time to write some code! We'll be using the same "stub"
with `CounterService` and `CreateServices` here:

``` cs --editable false --region Part03_CounterService --source-file Part03.cs
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

Here is how you use `MutableState<T>`:

``` cs --region Part03_MutableState --source-file Part03.cs
var services = CreateServices();
var stateFactory = services.GetStateFactory();
var state = stateFactory.NewMutable<int>(1);
var computed = state.Computed;
WriteLine($"Value: {state.Value}, Computed: {state.Computed}");
state.Value = 2;
WriteLine($"Value: {state.Value}, Computed: {state.Computed}");
WriteLine($"Old computed: {computed}"); // Should be invalidated
```

The output:

```text
Value: 1, Computed: StateBoundComputed`1(MutableState`1(#63646052) @2AULKFZxjG, State: Consistent)
Value: 2, Computed: StateBoundComputed`1(MutableState`1(#63646052) @2AULKFZxlK, State: Consistent)
Old computed: StateBoundComputed`1(MutableState`1(#63646052) @2AULKFZxjG, State: Invalidated)
```

Note that:
* `services.GetStateFactory()` is a shortcut for `services.GetRequiredService<IStateFactory>()`
* Old `computed` is in `Invalidated` state at the last line.

Let's look at error handling example:

``` cs --region Part03_MutableStateError --source-file Part03.cs
var services = CreateServices();
var stateFactory = services.GetStateFactory();
var state = stateFactory.NewMutable<int>();
var computed = state.Computed;
WriteLine($"Value: {state.Value}, Computed: {state.Computed}");
WriteLine("Setting state.Error.");
state.Error = new ApplicationException("Just a test");
try
{
    WriteLine($"Value: {state.Value}, Computed: {state.Computed}");
}
catch (ApplicationException)
{
    WriteLine($"Error: {state.Error.GetType()}, Computed: {state.Computed}");
}
WriteLine($"LastValue: {state.LastValue}, LastValueComputed: {state.LastValueComputed}");
```

The output:

```text
Value: 0, Computed: StateBoundComputed`1(MutableState`1(#63646052) @caJUviqcf, State: Consistent)
Setting state.Error.
Error: System.ApplicationException, Computed: StateBoundComputed`1(MutableState`1(#63646052) @caJUviqej, State: Consistent)
LastValue: 0, LastValueComputed: StateBoundComputed`1(MutableState`1(#63646052) @caJUviqcf, State: Invalidated)
```

As you see, `Value` property throws an exception here &ndash;
as per `IResult<T>` contract, it re-throws an exception stored
in `Error`.

The last "valid" value is still available via `LastValue` property;
similarly, the last "valid" computed instance is still available
via `LastValueComputed`.

## Live State

Let's play with `ILiveState<T>` now:

``` cs --region Part03_LiveState --source-file Part03.cs
var services = CreateServices();
var counters = services.GetService<CounterService>();
var stateFactory = services.GetStateFactory();
WriteLine("Creating aCounterState.");
var aCounterState = stateFactory.NewLive<string>(
    options => {
        options.WithUpdateDelayer(TimeSpan.FromSeconds(1)); // 1 second update delay
        options.Invalidated += state => WriteLine($"{DateTime.Now}: Invalidated, Computed: {state.Computed}");
        options.Updated += state => WriteLine($"{DateTime.Now}: Updated, Value: {state.Value}, Computed: {state.Computed}");
    },
    async (state, cancellationToken) => {
        var counter = await counters.GetAsync("a");
        return $"counters.GetAsync(a) -> {counter}";
    });
WriteLine("Before aCounterState.UpdateAsync(false).");
await aCounterState.UpdateAsync(false); // Ensures the state gets up-to-date value
WriteLine("After aCounterState.UpdateAsync(false).");
counters.Increment("a");
await Task.Delay(2000);
WriteLine($"Value: {aCounterState.Value}, Computed: {aCounterState.Computed}");
```

The output:

```text
Creating aCounterState.
9/2/2020 10:07:23 PM: Updated, Value: , Computed: StateBoundComputed`1(FuncLiveState`1(#64854219) @2C3RmseJCR, State: Consistent)
9/2/2020 10:07:23 PM: Invalidated, Computed: StateBoundComputed`1(FuncLiveState`1(#64854219) @2C3RmseJCR, State: Invalidated)
Before aCounterState.UpdateAsync(false).
GetAsync(a)
9/2/2020 10:07:23 PM: Updated, Value: counters.GetAsync(a) -> 0, Computed: StateBoundComputed`1(FuncLiveState`1(#64854219) @2C3RmseJEV, State: Consistent)
After aCounterState.UpdateAsync(false).
Increment(a)
9/2/2020 10:07:23 PM: Invalidated, Computed: StateBoundComputed`1(FuncLiveState`1(#64854219) @2C3RmseJEV, State: Invalidated)
GetAsync(a)
9/2/2020 10:07:24 PM: Updated, Value: counters.GetAsync(a) -> 1, Computed: StateBoundComputed`1(FuncLiveState`1(#64854219) @2C3RmseJCU, State: Consistent)
Value: counters.GetAsync(a) -> 1, Computed: StateBoundComputed`1(FuncLiveState`1(#64854219) @2C3RmseJCU, State: Consistent)
```

Some observations:

* New `ILiveState<T>` initially gets a default value
  (the first "Updated: ..." output)
* This value gets invalidated immediately (i.e. while
  `stateFactory.NewLive<T>(...)` runs)
* `aCounterState.UpdateAsync(false)` triggers its update.
  Interestingly, though, that this update will anyway happen
  immediately by default - the very first update delay is
  always zero. You can check this by replacing the line
  with `UpdateAsync(false)` to `await Task.Delay(100)`
* Later the invalidation happens right after the value
  state depends on gets invalidated.
* But the update follows in 1 second - i.e. as it was
  specified in options we've provided.

#### [Next: Part 4 &raquo;](./Part04.md) | [Tutorial Home](./README.md)

