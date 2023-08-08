namespace Samples.RpcBenchmark.Server;

public class TestService : ITestService
{
    public Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new HelloReply { Response = request.Request });

    public Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => Task.FromResult(userId > 0 ? User.ExamplePayload : null);

    public Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => Task.FromResult(a + b);
}
