namespace Samples.Benchmark.Client;

public abstract class BenchmarkWorker(ITestService client)
{
    public readonly ITestService Client = client;
    public readonly Random Random = new(Random.Shared.Next());
    public Func<CancellationToken, Task> Operation = null!;

    public async Task Initialize(ConcurrentQueue<int> remainingItemIds, CancellationToken cancellationToken)
    {
        var clock = SystemClock.Instance;
        while (remainingItemIds.TryDequeue(out var itemId)) {
            var item = await Client.TryGet(itemId, cancellationToken).ConfigureAwait(false);
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
            await Client.AddOrUpdate(item, null, cancellationToken).ConfigureAwait(false);
        }
    }
}
