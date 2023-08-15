using System.Net.Http;
using Grpc.Net.Client;
using MagicOnion.Client;

namespace Samples.RpcBenchmark.Client;

public class MagicOnionTestClient : ITestService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IMagicOnionTestService _client;

    public MagicOnionTestClient(IServiceProvider services)
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
        _client = MagicOnionClient.Create<IMagicOnionTestService>(channel);
    }

    public void Dispose()
        => _httpClient.Dispose();

    public async Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => await _client.SayHello(request);

    public async Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => await _client.GetUser(userId);

    public async Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => await _client.Sum(a, b);
}
