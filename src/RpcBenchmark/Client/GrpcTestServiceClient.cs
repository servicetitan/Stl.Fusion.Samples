using System.Net.Http;
using Grpc.Net.Client;

namespace Samples.RpcBenchmark.Client;

public class GrpcTestServiceClient : ITestService
{
    public readonly GrpcService.GrpcServiceClient Client;

    public GrpcTestServiceClient(IServiceProvider services)
    {
        var channelOptions = new GrpcChannelOptions() {
            HttpHandler = new SocketsHttpHandler {
                EnableMultipleHttp2Connections = true,
                MaxConnectionsPerServer = int.MaxValue,
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
            }
        };
        var channel = GrpcChannel.ForAddress(Settings.BaseUrl, channelOptions);
        Client = new(channel);
    }

    public Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
