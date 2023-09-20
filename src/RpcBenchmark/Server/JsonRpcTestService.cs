using Stl.Rpc;

namespace Samples.RpcBenchmark.Server;

public class JsonRpcTestService : ITestJsonRpcService
{
    public Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new HelloReply { Response = request.Request });

    public Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => Task.FromResult(userId > 0 ? Examples.User : null);

    public Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => Task.FromResult(a + b);

    public Task<RpcStream<Item>> GetItems(GetItemsRequest request, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    public IAsyncEnumerable<Item> GetItemsAlt(GetItemsRequest request, CancellationToken cancellationToken = default)
        => StreamGenerator.GetItems(request, cancellationToken);
}
