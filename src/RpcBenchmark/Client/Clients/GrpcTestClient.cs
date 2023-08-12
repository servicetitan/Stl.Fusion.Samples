using System.Net.Http;
using Grpc.Net.Client;

namespace Samples.RpcBenchmark.Client;

public class GrpcTestClient : ITestService, IDisposable
{
    private readonly HttpClient _httpClient;

    public readonly GrpcService.GrpcServiceClient Client;

    public GrpcTestClient(IServiceProvider services)
    {
        _httpClient = services.GetRequiredService<HttpClient>();
        var channelOptions = new GrpcChannelOptions() {
            HttpClient = _httpClient,
#if false
            HttpHandler = new SocketsHttpHandler {
                EnableMultipleHttp2Connections = true,
                MaxConnectionsPerServer = int.MaxValue,
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
            }
#endif
        };
        var channel = GrpcChannel.ForAddress(_httpClient.BaseAddress!, channelOptions);
        Client = new(channel);
    }

    public void Dispose()
        => _httpClient.Dispose();

    // gRPC test calls methods directly on the Client instead of the ones below

    public Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
