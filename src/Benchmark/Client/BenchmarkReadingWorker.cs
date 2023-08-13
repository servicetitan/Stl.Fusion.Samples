namespace Samples.Benchmark.Client;

public sealed class BenchmarkReadingWorker : BenchmarkWorker
{
    public BenchmarkReadingWorker(ITestService client) : base(client)
        => Operation = Read;

    private async Task Read(CancellationToken cancellationToken)
    {
        var itemId = (long)(1 + Random.Next(0, ItemCount));
        var item = await Client.TryGet(itemId, StopToken).ConfigureAwait(false);
        if (item?.Id != itemId)
            throw new InvalidOperationException();
    }
}
