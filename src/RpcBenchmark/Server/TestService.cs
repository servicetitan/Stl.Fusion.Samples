using Stl.Rpc;

namespace Samples.RpcBenchmark.Server;

public class TestService : ITestService
{
    public Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new HelloReply { Response = request.Request });

    public Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => Task.FromResult(userId > 0 ? Examples.User : null);

    public Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => Task.FromResult(a + b);

    public Task<RpcStream<Item>> GetItems(GetItemsRequest request, CancellationToken cancellationToken = default)
    {
        var stream = new RpcStream<Item>(StreamGenerator.GetItems(request, cancellationToken)) {
            AckPeriod = 1500,
            AckAdvance = 3001,
        };
        return Task.FromResult(stream);
    }
}
