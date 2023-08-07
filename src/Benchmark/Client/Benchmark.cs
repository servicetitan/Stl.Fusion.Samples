using static System.Console;

namespace Samples.Benchmark.Client;
using static Settings;

public class Benchmark
{
    public string Title { get; }
    public BenchmarkWorker[] Workers { get; }

    public Benchmark(string title, Func<ITestService> testServiceFactory, double workerCountMultiplier = 1)
    {
        Title = title;
        var workerCount = (int)(WorkerCount * workerCountMultiplier);
        Workers = new BenchmarkWorker[workerCount];
        var testService = (ITestService)null!;
        for (var i = 0; i < Workers.Length; i++) {
            if (i % TestServiceConcurrency == 0)
                testService = testServiceFactory.Invoke();
            Workers[i] = new BenchmarkWorker(this, testService, i);
        }
    }

    public async Task Initialize(CancellationToken cancellationToken = default)
    {
        WriteLine("Initializing...");
        var remainingItemIds = new ConcurrentQueue<int>(Enumerable.Range(1, ItemCount).ToArray());
        var tasks = Workers.Select(w => w.Initialize(remainingItemIds, cancellationToken)).ToArray();
        _ = Task.Run(async () => {
            while (!remainingItemIds.IsEmpty) {
                WriteLine($"  Remaining item count: {remainingItemIds.Count}");
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
            }
        }, cancellationToken);
        await Task.WhenAll(tasks).ConfigureAwait(false);
        WriteLine("  Done.");
    }

    public async Task Run(CancellationToken cancellationToken = default)
    {
        WriteLine($"  {Title} - {Workers.Length} workers");
        await RunWorkers(WarmupDuration, cancellationToken).ConfigureAwait(false);
        var counters = await RunWorkers(Duration, cancellationToken).ConfigureAwait(false);
        foreach (var (key, counter) in counters.OrderBy(p => p.Key))
            if (counter.HasValue)
                WriteLine($"  - {key,-8}: {counter.Format(Duration)}");
    }

    // Private methods

    private async Task<Dictionary<string, Counter>> RunWorkers(TimeSpan duration, CancellationToken cancellationToken)
    {
        if (ForceGCCollect) {
            GC.Collect();
            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            GC.Collect();
        }
        var whenReadySource = TaskCompletionSourceExt.New<CpuTimestamp>();
        var runTask = Workers
            .Select(w => Task.Run(() => w.Run(whenReadySource.Task, cancellationToken), CancellationToken.None))
            .ToList();
        whenReadySource.SetResult(CpuTimestamp.Now + duration);
        var counters = new Dictionary<string, Counter>();
        foreach (var task in runTask) {
            var runCounters = await task.ConfigureAwait(false);
            foreach (var (key, counter) in runCounters) {
                if (counters.TryGetValue(key, out var existingCounter))
                    counters[key] = existingCounter.MergeWith(counter);
                else
                    counters[key] = counter;
            }
        }
        return counters;
    }
}
