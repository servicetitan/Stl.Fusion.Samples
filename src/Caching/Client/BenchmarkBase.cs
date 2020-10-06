using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Stl.Async;
using Stl.OS;

namespace Samples.Caching.Client
{
    public abstract class BenchmarkBase
    {
        public int ConcurrencyLevel { get; set; } = HardwareInfo.ProcessorCount * 20;
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan WarmupDuration { get; set; } = TimeSpan.FromSeconds(1);
        public bool ForceGCCollect { get; set; }
        public int TimeCheckOperationIndexMask { get; set; } = 0;

        public long TotalOperationCount { get; private set; }
        public double OperationsPerSecond => TotalOperationCount / Duration.TotalSeconds;
        protected Stopwatch Stopwatch { get; private set; } = null!;

        public async Task RunAsync(string title, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Benchmarking '{title}':");
            await RunAsync(WarmupDuration, cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"  Parameters: {FormatParameters()}");
            await RunAsync(Duration, cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"  Speed:      {OperationsPerSecond / 1000:N3}K operations/s");
        }

        public virtual string FormatParameters()
            => $"{Duration.TotalSeconds:N}s x {ConcurrencyLevel} threads";

        protected virtual async Task RunAsync(TimeSpan duration, CancellationToken cancellationToken)
        {
            var startTaskSource = TaskSource.New<Unit>(true);
            var tasks = Enumerable.Range(0, ConcurrencyLevel)
                .Select(async i => {
                    await startTaskSource.Task.ConfigureAwait(false);
                    return await BenchmarkAsync(i, duration, cancellationToken).ConfigureAwait(false);
                })
                .ToArray();
            await Task.Delay(100, cancellationToken).ConfigureAwait(false); // Wait to make sure all the tasks are created & scheduled
            if (ForceGCCollect)
                GC.Collect();
            Stopwatch = Stopwatch.StartNew();
            startTaskSource.SetResult(default);

            var totalOperationCount = 0L;
            foreach (var task in tasks)
                totalOperationCount += await task.ConfigureAwait(false);
            TotalOperationCount = totalOperationCount;
        }

        protected abstract Task<long> BenchmarkAsync(int threadIndex, TimeSpan duration, CancellationToken cancellationToken);
    }
}
