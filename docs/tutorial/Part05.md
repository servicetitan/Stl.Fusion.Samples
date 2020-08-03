# Part 5: Computed Services: dependencies

> When a computed service method is executed, its output becomes
> dependent on every computed service output it calls.

Here is how it works:

``` cs --region part05_defineServices --source-file Part05.cs
public class UserRegistry
{
    private readonly ConcurrentDictionary<long, string> _userNames =
        new ConcurrentDictionary<long, string>();

    // Notice there is no [ComputedServiceMethod], because it doesn't
    // return anything on which other methods may depend
    public void SetUserName(long userId, string value)
    {
        _userNames[userId] = value;
        Computed.Invalidate(() => GetUserNameAsync(userId));
        WriteLine($"! {nameof(GetUserNameAsync)}({userId}) -> invalidated");
    }

    [ComputeMethod]
    public virtual async Task<string?> GetUserNameAsync(long userId)
    {
        WriteLine($"* {nameof(GetUserNameAsync)}({userId})");
        return _userNames.TryGetValue(userId, out var name) ? name : null;
    }
}

public class Clock
{
    // A better way to implement auto-invalidation:
    // uncomment the next line and comment out the line with "Task.Delay".
    // [ComputedServiceMethod(AutoInvalidateTime = 0.1)]
    [ComputeMethod]
    public virtual async Task<DateTime> GetTimeAsync()
    {
        WriteLine($"* {nameof(GetTimeAsync)}()");
        // That's how you "pull" the computed that is going to
        // store the result of this computation
        var computed = Computed.GetCurrent();
        // We just start this task here, but don't await for its result
        Task.Delay(TimeSpan.FromSeconds(0.1)).ContinueWith(_ =>
        {
            computed!.Invalidate();
            WriteLine($"! {nameof(GetTimeAsync)}() -> invalidated");
        }).Ignore();
        return DateTime.Now;
    }
}

public class FormatService
{
    private readonly UserRegistry _users;
    private readonly Clock _clock;

    public FormatService(UserRegistry users, Clock clock)
    {
        _users = users;
        _clock = clock;
    }

    [ComputeMethod]
    public virtual async Task<string> FormatUserNameAsync(long userId)
    {
        WriteLine($"* {nameof(FormatUserNameAsync)}({userId})");
        var userName = await _users.GetUserNameAsync(userId);
        var time = await _clock.GetTimeAsync();
        return $"{time:HH:mm:ss:fff}: User({userId})'s name is '{userName}'";
    }
}
```

We'll use a bit different container builder here:

``` cs --region part05_createServiceProvider --source-file Part05.cs
public static IServiceProvider CreateServiceProvider()
{
    var services = new ServiceCollection()
        .AddFusionCore()
        .AddComputeService<UserRegistry>()
        .AddComputeService<Clock>()
        .AddComputeService<FormatService>();

    return services.BuildServiceProvider();
}
```

Now, let's run some code:

``` cs --region part05_useServices_part1 --source-file Part05.cs
var services = CreateServiceProvider();
var users = services.GetRequiredService<UserRegistry>();
var formatter = services.GetRequiredService<FormatService>();

users.SetUserName(0, "John Carmack");
for (var i = 0; i < 5; i++)
{
    WriteLine(await formatter.FormatUserNameAsync(0));
    await Task.Delay(100);
}
users.SetUserName(0, "Linus Torvalds");
WriteLine(await formatter.FormatUserNameAsync(0));
users.SetUserName(0, "Satoshi Nakamoto");
WriteLine(await formatter.FormatUserNameAsync(0));
```

I hope it's clear how it works now:

* Any result produced by computed service gets cached till the moment
  it gets either invalidated or evicted. Eviction is possible only while
  no one uses or depends on it.
* Invalidations are always cascading, i.e. if A uses B, B uses C, and C
  gets invalidated, the whole chain gets invalidated.

Now, can we await for invalidation and update the computed instance?
Yes, we can!

``` cs --region part05_useServices_part2 --source-file Part05.cs
var services = CreateServiceProvider();
var users = services.GetRequiredService<UserRegistry>();
var formatter = services.GetRequiredService<FormatService>();

users.SetUserName(0, "John Carmack");
var cFormattedUser0 = await Computed.CaptureAsync(async _ =>
    await formatter.FormatUserNameAsync(0));
for (var i = 0; i < 10; i++)
{
    WriteLine(cFormattedUser0.Value);
    await cFormattedUser0.WhenInvalidatedAsync();
    // Note that nothing gets recomputed automatically;
    // on a positive side, any IComputed knows how to recompute itself,
    // so you can always do this manually:
    cFormattedUser0 = await cFormattedUser0.UpdateAsync(false);
}
```

That's all on computed services. Let's summarize the most important
pieces:

`IComputed<TOut>` has:

* `State` property, which transitions from
  `Computing` to `Computed` and `Invalidated`. It's fine, btw,
  to invalidate a computed instance in `Computing` state -
  this will trigger later invalidation, that will happen right
  after it enters `Computed` state.
* `IsConsistent` property - a shortcut to check whether its state
  is exactly `Consistent`
* `Output` property - it stores either a value or an error;
  its actual type is `Result<TOut>`.
* `Value` property - a shortcut to `Output.Value`
* `Invalidate()` method - turns computed into `Invalidated` state.
  You can call it multiple times, subsequent calls do nothing.
* `Invalidated` event - raised on invalidation. Handlers of this event
  should never throw exceptions; besides that, this event is raised
  first, and only after that the dependencies get similar `Invalidate()`
  call.
* `InvalidatedAsync` - an extension method that allows you to await
  for invalidation.
* `UpdateAsync` method - returns the most up-to-date (*most likely*,
  consistent - unless it was invalidated right after the update)
  computed instance for the same computation.
* `UseAsync` method - the same as `UpdateAsync(true).Value`.
  Gets the most up-to-date value of the current computed and
  makes sure that if this happens inside the computation of another
  computed, this "outer" computed registers the current one
  as its own dependency.

`IComputedService`:

* Is any type that implements this interface and is registered
  in `IServiceCollection` via `AddComputedService` extension method.
* It typically should

#### [Next: Part 6 &raquo;](./Part06.md) | [Tutorial Home](./README.md)

