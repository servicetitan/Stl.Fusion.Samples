# Part 4: Compute Services: execution, caching, and invalidation

Just a reminder, we're going to use the same "shortcut" to create an instance
of compute service:

``` cs --editable false --region part04_createHelper --source-file Part04.cs
public static TService Create<TService>()
    where TService : class
{
    var services = new ServiceCollection()
        .AddFusionCore()
        .AddComputeService<TService>();

    var provider = services.BuildServiceProvider();
    return provider.GetRequiredService<TService>();
}
```

## Call execution

> When you simultaneously call the same method of compute service
> with the same arguments from multiple threads, it's guaranteed
> that:
> 
> * At most one of these calls will be actually executed. All other  
>   calls will wait till the moment its result is ready, and once
>   it happens, they'll simply return this result.
> * At best none of these calls will be actually executed - in case
>   when the consistent call result for this set of arguments is
>   already cached.

Let's create a simple service to show this:

``` cs --region part04_defineCalculator --source-file Part04.cs
public class Calculator
{
    [ComputeMethod]
    public virtual async Task<double> SumAsync(double a, double b, bool logEnterExit = true)
    {
        if (logEnterExit)
            WriteLine($"+ {nameof(SumAsync)}({a}, {b})");
        await Task.Delay(100);
        if (logEnterExit)
            WriteLine($"- {nameof(SumAsync)}({a}, {b})");
        return a + b;
    }
}
```

And a simple test:

``` cs --region part04_defineTestCalculator --source-file Part04.cs
static async Task TestCalculator(Calculator calculator)
{
    WriteLine($"Testing '{calculator.GetType()}':");
    var tasks = new List<Task<double>> {
                calculator.SumAsync(1, 1),
                calculator.SumAsync(1, 1),
                calculator.SumAsync(1, 1),
                calculator.SumAsync(1, 2),
                calculator.SumAsync(1, 2)
            };
    await Task.WhenAll(tasks);
    var sum = tasks.Sum(t => t.Result);
    WriteLine($"Sum of results: {sum}");
}
```

Now, let's compare the behavior of a "normal" instance
and the instance provided by Fusion:

``` cs --region part04_useCalculator1 --source-file Part04.cs
var normalCalculator = new Calculator();
await TestCalculator(normalCalculator);

var fusionCalculator = Create<Calculator>();
await TestCalculator(fusionCalculator);
```

As you see, even though the final sum is the same, the way it works
is drastically different:

* The compute service version runs every computation just
  once for each unique set of arguments
* The computations for each unique set of arguments are running
  in parallel.

Let's check for how long it actually caches the results:

``` cs --region part04_useCalculator2 --source-file Part04.cs
var c = Create<Calculator>();
for (var i = 0; i < 10; i++)
{
    await TestCalculator(c);
    await Task.Delay(1000);
}
```

So everything stays in cache here. Let's check out how caching works.

## Cache invalidation & eviction

> Every result of `[ComputeMethod]` call is cached:
> 
> * (this, methodInfo, arguments) is the key; in reality,
>   it's a bit more complex: e.g. `CancellationToken` argument
>   is always ignored. `IArgumentComparerProvider` (one of services
>   you can register in the container) decides how to compare the keys,
>   but the default implementation always relies on `object.Equals`.
> * `IComputed<TOut>` is the value that's cached. It is
>   always strongly referenced for at least `ComputedOptions.KeepAliveTime`
>   after the last access operation (cache hit for this value),
>   and weakly referenced afterwards until the moment it gets invalidated.
>   Once invalidated, it gets evicted from the cache.
>   Weak referencing ensures there is one and only one valid instance
>   of every `IComputed` produced for a certain call.

Let's try to make Fusion to evict some cached entries:

``` cs --region part04_useCalculator3 --source-file Part04.cs
var c = Create<Calculator>();
await TestCalculator(c);

// Default ComputedOptions.KeepAliveTime is 1s, we need to
// wait at least this time to make sure the following Prune call
// will evict the entry.
await Task.Delay(1100);
var registry = ComputedRegistry.Default;
var mPrune = registry.GetType().GetMethod("Prune", BindingFlags.Instance | BindingFlags.NonPublic);
mPrune!.Invoke(registry, Array.Empty<object>());
GC.Collect();

await TestCalculator(c);
```

`ComputedRegistry.Prune` removes strong references to the
entries with expired `KeepAliveTime` (which is 1 second by default)
and removes the entries those `IComputed` instances are collected by GC.
We invoked it in above example to ensure strong references to our cached
computed instances (method outputs) are removed, so GC can pick it up.

`Prune` is triggered once per `O(registry.Capacity)` operations (reads and
updates) with `ComputedRegistry`. This ensures the amortized cost of pruning
is `O(1)` per operation. So in reality, you don't have to invoke it manually -
it will be invoked after a certain number of operations anyway:

``` cs --region part04_useCalculator4 --source-file Part04.cs
var c = Create<Calculator>();
await TestCalculator(c);

await Task.Delay(1100);
var tasks = new List<Task>();
for (var i = 0; i < 200_000; i++)
    tasks.Add(c.SumAsync(3, i, false));
await Task.WhenAll(tasks);
GC.Collect();

await TestCalculator(c);
```

Ok, now let's check out how the invalidation impacts our cache.
Can we somehow pull an instance of `IComputed` that represents
the result of specific call and invalidate it manually?

Yes:

``` cs --region part04_useCalculator5 --source-file Part04.cs
var calc = Create<Calculator>();

var s1 = await calc.SumAsync(1, 1);
WriteLine($"{nameof(s1)} = {s1}");

// Now let's pull the computed instance that represents the result of
// Notice that the underlying SumAsync code won't be invoked,
// since the result is already cached:
var c1 = await Computed.CaptureAsync(_ => calc.SumAsync(1, 1));
WriteLine($"{nameof(c1)} = {c1}, Value = {c1.Value}");

// And invalidate it
c1.Invalidate();
WriteLine($"{nameof(c1)} = {c1}, Value = {c1.Value}");

// Let's compute the sum once more now.
// You'll see that SumAsync gets invoked this time.
s1 = await calc.SumAsync(1, 1);
WriteLine($"{nameof(s1)} = {s1}");
```

There is a shorter way to invalidate method call result:

``` cs --region part04_useCalculator6 --source-file Part04.cs
var calc = Create<Calculator>();

WriteLine("Calling & invalidating SumAsync(1, 1)");
WriteLine(await calc.SumAsync(1, 1));
// This will lead to re-computation
Computed.Invalidate(() => calc.SumAsync(1, 1));
WriteLine(await calc.SumAsync(1, 1));

WriteLine("Calling SumAsync(2, 2), but invalidating SumAsync(2, 3)");
WriteLine(await calc.SumAsync(2, 2));
// But this won't - because the arguments are (2, 3), not (2, 2)
Computed.Invalidate(() => calc.SumAsync(2, 3));
WriteLine(await calc.SumAsync(2, 2));
```

#### [Next: Part 5 &raquo;](./Part05.md) | [Tutorial Home](./README.md)

