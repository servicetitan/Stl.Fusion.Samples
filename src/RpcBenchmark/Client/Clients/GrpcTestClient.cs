using Grpc.Net.Client;
using Stl.Rpc;

namespace Samples.RpcBenchmark.Client;

public class GrpcTestClient(GrpcChannel grpcChannel) : ITestService, IDisposable
{
    public readonly GrpcService.GrpcServiceClient Client = new(grpcChannel);

    public void Dispose()
        => grpcChannel.Dispose();

    // gRPC test calls methods directly on the Client instead of the ones below

    public Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<RpcStream<Item>> GetItems(GetItemsRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
