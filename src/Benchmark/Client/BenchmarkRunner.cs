using Stl.Benchmarking;

namespace Samples.Benchmark.Client;
using static Settings;

public class BenchmarkRunner : BenchmarkRunnerBase<double>
{
    private BenchmarkWorker[] Workers { get; }

    public BenchmarkRunner(string title, Func<ITestService> clientFactory, double readerCountMultiplier = 1)
    {
        var readerCount = ReaderCount * readerCountMultiplier;
        var workerCount = (int)(WriterCount + readerCount);
        Title = $"{title,-32} {readerCount,5} readers";
        TryCount = Settings.TryCount;
        ResultFormatter = x => $"{x.FormatCount(),7}";
        Workers = new BenchmarkWorker[workerCount];
        var client = (ITestService)null!;
        for (var i = 0; i < Workers.Length; i++) {
            if (i % TestServiceConcurrency == 0)
                client = clientFactory.Invoke();
            if (i < WriterCount)
                Workers[i] = new BenchmarkWritingWorker(client);
            else
                Workers[i] = new BenchmarkReadingWorker(client);
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

    // Protected & private methods

    protected override Task Warmup(CancellationToken cancellationToken)
        => GetCallFrequency(WarmupDuration, cancellationToken);

    protected override Task<double> Benchmark(CancellationToken cancellationToken)
        => GetCallFrequency(Duration, cancellationToken);

    private Task<double> GetCallFrequency(double duration, CancellationToken cancellationToken)
        => Benchmarks.CallFrequency(Workers, duration, cancellationToken,
            w => w.Operation, null, w => w is BenchmarkWritingWorker);
}
