namespace Samples.RpcBenchmark.Client;

public class BenchmarkWorker(ITestService client)
{
    protected const int StreamLength = 10;

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
            DataSize = 0,
            DelayEvery = 1,
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
            DataSize = 0,
            DelayEvery = 1,
            Count = StreamLength,
        };
        var items = await Client.GetItems(request, cancellationToken).ConfigureAwait(false);
        var count = await items.CountAsync(cancellationToken).ConfigureAwait(false);
        if (count != StreamLength)
            throw new InvalidOperationException("Wrong result.");
    }
}
