using Stl.Rpc;

namespace Samples.RpcBenchmark;

public interface ITestService : IRpcService
{
    Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default);
    Task<User?> GetUser(long userId, CancellationToken cancellationToken = default);
    Task<int> Sum(int a, int b, CancellationToken cancellationToken = default);
    Task<RpcStream<Item>> GetItems(GetItemsRequest request, CancellationToken cancellationToken = default);
}
