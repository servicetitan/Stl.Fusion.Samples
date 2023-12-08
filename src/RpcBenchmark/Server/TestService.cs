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
        const int ackPeriod = 900;
        var stream = new RpcStream<Item>(StreamGenerator.GetItems(request, cancellationToken)) {
            AckPeriod = ackPeriod,
            AckAdvance = (ackPeriod * 2) + 1,
        };
        return Task.FromResult(stream);
    }
}
