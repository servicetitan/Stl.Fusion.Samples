using System.Net.WebSockets;
using Stl.Rpc;
using StreamJsonRpc;

namespace Samples.RpcBenchmark.Client;

public class StreamJsonRpcTestClient : ITestService, IHasWhenReady, IDisposable
{
    private readonly ClientWebSocket _webSocket;
    private ITestJsonRpcService _client = null!;

    public Task WhenReady { get; }

    public StreamJsonRpcTestClient(IServiceProvider services)
    {
        var baseUrl = services.GetRequiredService<ClientFactories>().BaseUrl;
        baseUrl = baseUrl.Replace("http://", "ws://").Replace("https://", "wss://");
        _webSocket = services.GetRequiredService<ClientWebSocket>();
        WhenReady = Task.Run(async () => {
            await _webSocket.ConnectAsync(new Uri($"{baseUrl}stream-json-rpc"), CancellationToken.None);
            var webSocketMessageHandler = new WebSocketMessageHandler(_webSocket);
            _client = JsonRpc.Attach<ITestJsonRpcService>(webSocketMessageHandler);
        });
    }

    public void Dispose()
        => _webSocket.Dispose();

    public Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => _client.SayHello(request, cancellationToken);

    public Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => _client.GetUser(userId, cancellationToken);

    public Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => _client.Sum(a, b, cancellationToken);

    public Task<RpcStream<Item>> GetItems(GetItemsRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new RpcStream<Item>(_client.GetItemsAlt(request, cancellationToken)));
}
