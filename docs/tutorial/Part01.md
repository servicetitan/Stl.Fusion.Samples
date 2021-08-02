# Part 1: Compute Services

Video covering this part:

[<img src="./img/Part1-Screenshot.jpg" width="200"/>](https://youtu.be/G-MIdfDP3gI)

Fusion offers 3 key abstractions enabling you to build real-time services:

1. **Computed Value** &ndash; an object describing the result of a computation
   of type `T`, which is also capable of notifying you when this result
   becomes *invalidated* (most likely inconsistent with the ground truth).
   Such values always implement `IComputed<T>`; you don't need to implement
   this interface though, because the most useful implementations of it
   are already there.
2. **Compute Service** &ndash; a service that automatically captures
   dependencies of outputs of its methods and transparently "backs" them with
   `IComputed<T>` instances allowing anyone to learn when such outputs become
   invalidated (inconsistent with the ground truth).
   Compute Services are supposed to be written by you.
3. **State** &ndash; an abstraction that "tracks" a single `IComputed<T>`,
   i.e. continuously references the most up-to-date version of it.
   Again, you typically don't need to implement your own `IState<T>` -
   Fusion provides its 3 most useful flavors.

Since Compute Services is what you mostly have to deal with,
let's start from this part.

But first, let's create a helper method allowing us to create an
`IServiceProvider` hosting our Compute Services:

``` cs --editable false --region Part01_CreateServices --source-file Part01.cs
public static IServiceProvider CreateServices()
{
    var services = new ServiceCollection();
    var fusion = services.AddFusion();
    fusion.AddComputeService<CounterService>();
    fusion.AddComputeService<CounterSumService>(); // We'll be using it later
    fusion.AddComputeService<HelloService>();      // We'll be using it later
    return services.BuildServiceProvider();
}
```

Now we're ready to declare our first Compute Service:

``` cs --editable false --region Part01_CounterService --source-file Part01.cs
public class CounterService
{
    private readonly ConcurrentDictionary<string, int> _counters = new ConcurrentDictionary<string, int>();

    [ComputeMethod]
    public virtual async Task<int> Get(string key)
    {
        WriteLine($"Get ({key})");
        return _counters.TryGetValue(key, out var value) ? value : 0;
    }

    public void Increment(string key)
    {
        WriteLine($"Increment ({key})");
        _counters.AddOrUpdate(key, k => 1, (k, v) => v + 1);
        using (Computed.Invalidate())
            Get(key).Ignore();
    }
}
```

For now, please ignore the fact `Get` is declared as asynchronous method,
even though it isn't truly asynchronous - later I'll explain why it's reasonable.

Let's use `CounterService`:

``` cs --region Part01_UseCounterService1 --source-file Part01.cs
var counters = CreateServices().GetRequiredService<CounterService>();
WriteLine(await counters.Get("a"));
WriteLine(await counters.Get("b"));
```

The output should be:

```text
Get(a)
0
Get(b)
0
```

It looks normal, right? But how about this:

``` cs --region Part01_UseCounterService2 --source-file Part01.cs
var counters = CreateServices().GetRequiredService<CounterService>();
WriteLine(await counters.Get("a"));
WriteLine(await counters.Get("a"));
```

The output looks weird now:

```text
Get(a)
0
0
```

So why "Get(a)" wasn't printed twice here? The answer is:

* You may think *any compute method automatically caches its output*.
* The cache key is `(MethodInfo, this, argument1, argument2, ...)`
* The cached value is method output
* The entry expires once you call `Computed.Invalidate(() => ...)`
  for the same method of the same service with the same set of arguments.

Let's see how it works:

``` cs --region Part01_UseCounterService3 --source-file Part01.cs
var counters = CreateServices().GetRequiredService<CounterService>();
WriteLine(await counters.Get("a"));
counters.Increment("a");
WriteLine(await counters.Get("a"));
```

The output:

```text
Get(a)
0
Increment(a)
Get(a)
1
```

Check out `CounterService.Increment` source code above - it calls
`Computed.Invalidate`, which evicts the entry. This explains why
in this example "Get(a)" is printed twice, even though previously
it was printed just for the first call.

## Dependencies

Now let's add another Compute Service:

``` cs --editable false --region Part01_CounterSumService --source-file Part01.cs
public class CounterSumService
{
    public CounterService Counters { get; }

    public CounterSumService(CounterService counters) => Counters = counters;

    [ComputeMethod]
    public virtual async Task<int> Sum(string key1, string key2)
    {
        WriteLine($"Sum({key1}, {key2})");
        return await Counters.Get(key1) + await Counters.Get(key2);
    }
}
```

And use it:

``` cs --region Part01_UseCounterSumService1 --source-file Part01.cs
var services = CreateServices();
var counterSum = services.GetRequiredService<CounterSumService>();
WriteLine(await counterSum.Sum("a", "b"));
WriteLine(await counterSum.Sum("a", "b"));
```

The output:

```text
Sum(a, b)
Get(a)
Get(b)
0
```

Assuming you know how these services work now, this is exactly what you'd expect.

Another example:

``` cs --region Part01_UseCounterSumService2 --source-file Part01.cs
var services = CreateServices();
var counterSum = services.GetRequiredService<CounterSumService>();
WriteLine("Nothing is cached (yet):");
WriteLine(await counterSum.Sum("a", "b"));
WriteLine("Only Get(a) and Get(b) outputs are cached:");
WriteLine(await counterSum.Sum("b", "a"));
WriteLine("Everything is cached:");
WriteLine(await counterSum.Sum("a", "b"));
```

The output:

```text
Nothing is cached (yet):
Sum(a, b)
Get(a)
Get(b)
0
Only Get(a) and Get(b) results are cached:
Sum(b, a)
0
Everything is cached:
0
```

Again, nothing unexpected. The results are still cached, but since the
key is sensitive to the order of arguments, entries for `("a", "b")` and
`("b", "a")` differ.

But what about this?

``` cs --region Part01_UseCounterSumService3 --source-file Part01.cs
var services = CreateServices();
var counters = services.GetRequiredService<CounterService>();
var counterSum = services.GetRequiredService<CounterSumService>();
WriteLine(await counterSum.Sum("a", "b"));
counters.Increment("a");
WriteLine(await counterSum.Sum("a", "b"));
```

The output:

```text
Sum(a, b)
Get(a)
Get(b)
0
Increment(a)
Sum(a, b)
Get(a)
1
```

This is quite unusual, right? *Somehow* `Sum("a", "b")` figured out that
it has to refresh `Get("a")` result first, because it was invalidated
due to increment. But how?

In reality, every compute method either gets a cached output, or builds
a new `IComputed<T>` instance "backing" the computation it's going to run,
and *while the computation runs*, this instance stays available via
`Computed.GetCurrent()` method. So any other compute method *invoked
during the computation* gets a chance to enlist its own hidden output
(`IComputed<T>` as well) as a *dependency* of the current computed instance.

The actual process is a bit more complex, because it accounts for
scenarios you may not anticipate yet:

- Recursion and multiple levels of compute method calls are fully supported
- Some results can be invalidated right during their computation
- No more than one computation for each distinct result should
  be running at any given moment.

To close this section, let's look at the last property closer.

## Concurrent Evaluations

Let's create a simple service to test how Fusion handles concurrency:

``` cs --editable false --region Part01_HelloService --source-file Part01.cs
public class HelloService
{
    [ComputeMethod]
    public virtual async Task<string> Hello(string name)
    {
        WriteLine($"+ Hello({name})");
        await Task.Delay(1000);
        WriteLine($"- Hello({name})");
        return $"Hello, {name}!";
    }
}
```

As you see, `Hello` method simply returns a formatted "Hello, X!" message,
but with a 1-second delay. Let's try to run it concurrently:

``` cs --region Part01_UseHelloService1 --source-file Part01.cs
var hello = CreateServices().GetRequiredService<HelloService>();
var t1 = Task.Run(() => hello.Hello("Alice"));
var t2 = Task.Run(() => hello.Hello("Bob"));
var t3 = Task.Run(() => hello.Hello("Bob"));
var t4 = Task.Run(() => hello.Hello("Alice"));
await Task.WhenAll(t1, t2, t3, t4);
WriteLine(t1.Result);
WriteLine(t2.Result);
WriteLine(t3.Result);
WriteLine(t4.Result);
```

The output:

```text
+ Hello(Bob)
+ Hello(Alice)
- Hello(Bob)
- Hello(Alice)
Hello, Alice!
Hello, Bob!
Hello, Bob!
Hello, Alice!
```

As you see, even though all 4 values were computed, there were just
2 `Hello` evaluations (for distinct arguments only), and moreover,
these two evaluations were running concurrently with each other.

This is an expected behavior: even though nothing is cached in the
beginning, there is no reason to run more than one computation
for e.g. "Bob" argument concurrently, since all of them are supposed
to produce the same result. This is exactly what Fusion ensures.

And on contrary, it's totally reasonable to let `Hello("Alice")`
computation to run concurrently with `Hello("Bob")`, because they
might produce different output, and if they were launched concurrently,
`HelloService` is designed to support this.

Overall, nearly everything in Fusion supports concurrent invocations:

- Compute Services are supposed to be singletons that support concurrency
- Any `IComputed<T>` implementation is fully concurrent
- As well as any `IState<T>`
- The exceptions are mostly such types as `XxxOptions` and methods that
  are supposed to be used during the service registration stage, as well
  as types that aren't supposed to be concurrent (e.g. all Blazor components - `StatefulComponentBase<TState>` and its descendants).

#### [Next: Part 2 &raquo;](./Part02.md) | [Tutorial Home](./README.md)

