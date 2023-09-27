namespace Samples.RpcBenchmark.Client;

public class BenchmarkWorker(ITestService client)
{
    public const int StreamLength = 30*60; // 30s * 60fps
    public const int DataSizeS = 0;
    public const int DataSizeL = 100;
    public const int DelayEveryS = 0;
    public const int DelayEveryL = 0;

    public readonly ITestService Client = client;

    public Task WhenReady()
        => Client is IHasWhenReady hasWhenReady
            ? hasWhenReady.WhenReady
            : Task.CompletedTask;

    public virtual async Task SayHello(CancellationToken cancellationToken)
    {
        var result = await Client.SayHello(Examples.HelloRequest, cancellationToken).ConfigureAwait(false);
        if (result.Response.Int32 != 32)
            throw new InvalidOperationException("Wrong result.");
    }

    public virtual async Task GetUser(CancellationToken cancellationToken)
    {
        var user = await Client.GetUser(1, cancellationToken).ConfigureAwait(false);
        if (user!.Id != 1)
            throw new InvalidOperationException("Wrong result.");
    }

    public virtual async Task Sum(CancellationToken cancellationToken)
    {
        var sum = await Client.Sum(1, 2, cancellationToken).ConfigureAwait(false);
        if (sum != 3)
            throw new InvalidOperationException("Wrong result.");
    }

    public virtual async Task StreamS(CancellationToken cancellationToken)
    {
        var request = new GetItemsRequest() {
            DataSize = DataSizeS,
            DelayEvery = DelayEveryS,
            Count = StreamLength,
        };
        var items = await Client.GetItems(request, cancellationToken).ConfigureAwait(false);
        var count = await items.CountAsync(cancellationToken).ConfigureAwait(false);
        if (count != StreamLength)
            throw new InvalidOperationException("Wrong result.");
    }

    public virtual async Task StreamL(CancellationToken cancellationToken)
    {
        var request = new GetItemsRequest() {
            DataSize = DataSizeL,
            DelayEvery = DelayEveryL,
            Count = StreamLength,
        };
        var items = await Client.GetItems(request, cancellationToken).ConfigureAwait(false);
        var count = await items.CountAsync(cancellationToken).ConfigureAwait(false);
        if (count != StreamLength)
            throw new InvalidOperationException("Wrong result.");
    }
}
