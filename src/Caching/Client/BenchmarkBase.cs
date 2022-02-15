using System.Collections.Generic;
using System.Diagnostics;
using Stl.OS;
using static System.Console;

namespace Samples.Caching.Client;

public abstract class BenchmarkBase
{
    public int ConcurrencyLevel { get; set; } = HardwareInfo.ProcessorCount;
    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan WarmupDuration { get; set; } = TimeSpan.FromSeconds(1);
    public bool ForceGCCollect { get; set; }
    public int TimeCheckOperationIndexMask { get; set; } = 0;

    protected Stopwatch Stopwatch { get; set; } = null!;
    protected Dictionary<string, Counter> Counters { get; set; } = new Dictionary<string, Counter>();

    public async Task Run(string title, CancellationToken cancellationToken = default)
    {
        WriteLine($"{title}:");
        await Run(WarmupDuration, cancellationToken).ConfigureAwait(false);
        await Run(Duration, cancellationToken).ConfigureAwait(false);
        foreach (var (key, counter) in Counters.OrderBy(p => p.Key))
            if (counter.HasValue)
                WriteLine($"  {key,-14}: {counter.Format(Duration)}");
    }

    public virtual void DumpParameters()
    {
        WriteLine("Benchmark parameters:");
        WriteLine($"  {"Duration",-14}: {Duration.TotalSeconds:N}s");
        WriteLine($"  {"Worker #",-14}: {ConcurrencyLevel}");
    }

    protected virtual async Task Run(TimeSpan duration, CancellationToken cancellationToken)
    {
        Counters.Clear();
        var startTaskSource = TaskSource.New<Unit>(true);
        var tasks = Enumerable.Range(0, ConcurrencyLevel)
            .Select(async i => {
                await startTaskSource.Task.ConfigureAwait(false);
                return await Benchmark(i, duration, cancellationToken).ConfigureAwait(false);
            })
            .ToArray();
        await Task.Delay(100, cancellationToken).ConfigureAwait(false); // Wait to make sure all the tasks are created & scheduled
        if (ForceGCCollect)
            GC.Collect();

        Stopwatch = Stopwatch.StartNew();
        startTaskSource.SetResult(default);

        foreach (var task in tasks) {
            var counters = await task.ConfigureAwait(false);
            foreach (var (key, counter) in counters) {
                if (Counters.TryGetValue(key, out var existingCounter))
                    Counters[key] = existingCounter.CombineWith(counter);
                else
                    Counters[key] = counter;
            }
        }
    }

    protected abstract Task<Dictionary<string, Counter>> Benchmark(int workerId, TimeSpan duration, CancellationToken cancellationToken);
}
