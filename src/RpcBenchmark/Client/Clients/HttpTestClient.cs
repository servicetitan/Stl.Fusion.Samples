using RestEase;
using Stl.Rpc;

namespace Samples.RpcBenchmark.Client;

public class HttpTestClient(IServiceProvider services) : ITestService
{
    private readonly ITestServiceClientDef _client = services.GetRequiredService<ITestServiceClientDef>();

    public Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => _client.SayHello(request, cancellationToken);

    public Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => _client.GetUser(userId, cancellationToken);

    public Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => _client.Sum(a, b, cancellationToken);

    public Task<RpcStream<Item>> GetItems(GetItemsRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
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
