using Grpc.Net.Client;
using MagicOnion.Client;
using Stl.Rpc;

namespace Samples.RpcBenchmark.Client;

public class MagicOnionTestClient(GrpcChannel grpcChannel) : ITestService, IDisposable
{
    private readonly IMagicOnionTestService _client = MagicOnionClient.Create<IMagicOnionTestService>(grpcChannel);

    public void Dispose()
        => grpcChannel.Dispose();

    public async Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => await _client.SayHello(request);

    public async Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => await _client.GetUser(userId);

    public async Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => await _client.Sum(a, b);

    public Task<RpcStream<Item>> GetItems(GetItemsRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
