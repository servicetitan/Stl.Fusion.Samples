namespace Samples.Benchmark.Client;

public sealed class BenchmarkWritingWorker : BenchmarkWorker
{
    public BenchmarkWritingWorker(ITestService client) : base(client)
        => Operation = Write;

    private async Task Write(CancellationToken cancellationToken)
    {
        try {
            var itemId = (long)(1 + Random.Next(0, ItemCount));
            var item = await Client.TryGet(itemId, StopToken).ConfigureAwait(false);
            item = item! with { Name = $"Item-{item.Id}-{item.Version + 1}" };
            await Client.AddOrUpdate(item, item.Version, StopToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch {
            // Intended
        }
    }
}
