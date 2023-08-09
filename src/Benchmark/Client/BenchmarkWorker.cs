namespace Samples.Benchmark.Client;
using static Settings;

public class BenchmarkWorker
{
    public readonly Benchmark Benchmark;
    public readonly int Index;
    public readonly ITestService TestService;
    public readonly bool IsWriter;
    public readonly Random Random;

    public BenchmarkWorker(Benchmark benchmark, ITestService testService, int index)
    {
        Benchmark = benchmark;
        Index = index;
        TestService = testService;
        IsWriter = WriterFrequency is { } f && index % f == 0;
        Random = new Random((index + 1)*347);
    }

    public virtual async Task Initialize(ConcurrentQueue<int> remainingItemIds, CancellationToken cancellationToken)
    {
        var clock = SystemClock.Instance;
        while (remainingItemIds.TryDequeue(out var itemId)) {
            var item = await TestService.TryGet(itemId, cancellationToken).ConfigureAwait(false);
            if (item != null)
                continue;

            var now = clock.Now.ToDateTime();
            item = new TestItem() {
                Id = itemId,
                Version = 1,
                CreatedAt = now,
                ModifiedAt = now,
                Name = $"Item-{itemId}",
            };
            await TestService.AddOrUpdate(item, null, cancellationToken).ConfigureAwait(false);
        }
    }

    public virtual async Task<Dictionary<string, Counter>> Run(
        Task<CpuTimestamp> whenReady, CancellationToken cancellationToken)
    {
        var count = 0L;
        var errorCount = 0L;
        var endsAt = await whenReady.ConfigureAwait(false);
        while ((count & TimeCheckCountMask) != 0 || CpuTimestamp.Now < endsAt) {
            count++;
            try {
                var itemId = (long)(1 + Random.Next(0, ItemCount));
                var item = await TestService.TryGet(itemId, cancellationToken).ConfigureAwait(false);
                if (IsWriter) {
                    item = item! with { Name = $"Item-{item.Id}-{item.Version + 1}" };
                    await TestService.AddOrUpdate(item, item.Version, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) {
                break;
            }
            catch (Exception) {
                errorCount++;
            }
        }
        return new Dictionary<string, Counter>() {
            { IsWriter ? "Writes" : "Reads", new OpsCounter(count - errorCount) },
            { IsWriter ? "!Writes" : "!Reads", new OpsCounter(errorCount) }
        };
    }
}
