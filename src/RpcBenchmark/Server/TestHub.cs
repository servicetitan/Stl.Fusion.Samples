using Microsoft.AspNetCore.SignalR;

namespace Samples.RpcBenchmark.Server;

public class TestHub(TestService service) : Hub
{
    public Task<HelloReply> SayHello(HelloRequest request)
        => service.SayHello(request);

    public Task<User?> GetUser(long userId)
        => service.GetUser(userId);

    public Task<int> Sum(int a, int b)
        => service.Sum(a, b);

    public IAsyncEnumerable<Item> GetItems(GetItemsRequest request, CancellationToken cancellationToken = default)
        => StreamGenerator.GetItems(request, cancellationToken);
}
