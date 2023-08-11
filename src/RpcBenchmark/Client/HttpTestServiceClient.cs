using RestEase;

namespace Samples.RpcBenchmark.Client;

public class HttpTestServiceClient(ITestServiceClientDef client) : ITestService
{
    public Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => client.SayHello(request, cancellationToken);

    public Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => client.GetUser(userId, cancellationToken);

    public Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => client.Sum(a, b, cancellationToken);
}

[BasePath("api/testService")]
public interface ITestServiceClientDef
{
    [Post(nameof(SayHello))]
    Task<HelloReply> SayHello([Body] HelloRequest request, CancellationToken cancellationToken = default);
    [Get(nameof(GetUser))]
    Task<User?> GetUser(long userId, CancellationToken cancellationToken = default);
    [Get(nameof(Sum))]
    Task<int> Sum(int a, int b, CancellationToken cancellationToken = default);
}
