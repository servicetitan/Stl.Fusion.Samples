# Part 2: Dependencies between computed instances

Computed instances describe computations, and if these computations
involve other computed instances, the computation that uses another
one is called "dependent", and the used one is called a "dependency".

The following rules apply:

* Once a dependency ("used" instance) gets invalidated, all the computed instances
  that depend on it (directly and indirectly) are invalidated as well.
  This happens *synchronously* right when you call `dependency.Invalidate()`.
* Once a dependent instance gets (re)computed, it triggers computation of all its
  dependencies (unless they are already computing or computed, i.e. their most
  recently produced `IComputed` instance is not in `Invalidated` state).

The code below (sorry, it's large) explains how it works:

``` cs --region part02_dependencies --source-file Part02.cs
WriteLine("Creating computed instances...");
var cDate = SimpleComputed.New<DateTime>(async (prev, ct) =>
{
    var result = DateTime.Now;
    WriteLine($"Computing cDate: {result}");
    return result;
});
var cCount = SimpleComputed.New<int>(async (prev, ct) =>
{
    var result = prev.Value + 1;
    WriteLine($"Computing cCount: {result}");
    return result;
});
var cTitle = SimpleComputed.New<string>(async (prev, ct) =>
{
    var date = await cDate.UseAsync(ct);
    var count = await cCount.UseAsync(ct);
    var result = $"{date}: {count}";
    WriteLine($"Computing cTitle: {result}");
    return result;
});

WriteLine("All the computed values below should be in invalidated state.");
WriteLine($"{cDate}, Value = {cDate.Value}");
WriteLine($"{cCount}, Value = {cCount.Value}");
WriteLine($"{cTitle}, Value = {cTitle.Value}");

WriteLine();
WriteLine("Let's trigger the computations:");
cTitle = await cTitle.UpdateAsync(false);
WriteLine($"{cDate}, Value = {cDate.Value}");
WriteLine($"{cCount}, Value = {cCount.Value}");
WriteLine($"{cTitle}, Value = {cTitle.Value}");

WriteLine();
WriteLine($"The next line won't trigger the computation, even though {nameof(cCount)} will be updated:");
cCount = await cCount.UpdateAsync(false);
WriteLine($"Let's do the same for {nameof(cDate)} now:");
cDate = await cDate.UpdateAsync(false);

WriteLine();
WriteLine($"Let's invalidate {nameof(cCount)} and see what happens:");
cCount.Invalidate();
WriteLine($"{cCount}, Value = {cCount.Value}");
WriteLine($"As you see, no computation is triggered so far.");
WriteLine($"But notice that {nameof(cTitle)} is invalidated as well, because it depends on {nameof(cCount)}:");
WriteLine($"{cTitle}, Value = {cTitle.Value}");

WriteLine($"Finally, let's update {nameof(cTitle)} again:");
cTitle = await cTitle.UpdateAsync(false);
WriteLine($"{cTitle}, Value = {cTitle.Value}");
```

You might notice that dependent-dependency links form a
[directed acyclic graph](https://en.wikipedia.org/wiki/Directed_acyclic_graph),
thus:

* There must be no cycles. Note, though, that the links are established
  between the instances, not the computations, so technically you're
  totally fine to have e.g. recursive functions that return computed instances.
* If we draw the graph so that the least dependent (most used ones / low-level logic)
  instances are at the bottom, and the most dependent ones (least used / high-level logic)
  are at the top,
  * Invalidation of a graph node also "spreads" to every node in its "upper subtree"
  * Computation of a graph node also "spreads" to every node in its "lower subtree".

#### [Next: Part 3 &raquo;](./Part03.md) | [Tutorial Home](./README.md)

