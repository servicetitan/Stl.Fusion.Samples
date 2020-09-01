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

``` cs --region Part02_PullComputed --source-file Part02.cs
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

As you may notice, it has:

- Some representation of input: `Intercepted:CounterService.GetAsync(a`
- Version: `@xIs0saqEU`
- State: `Consistent`
- Value: `0`

In reality, there is much more, but these are the key properties.

#### [Next: Part 3 &raquo;](./Part02.md) | [Tutorial Home](./README.md)

