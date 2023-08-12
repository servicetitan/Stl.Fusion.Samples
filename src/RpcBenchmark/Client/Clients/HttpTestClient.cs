using System.Net.Http;
using RestEase;
using Stl.RestEase;

namespace Samples.RpcBenchmark.Client;

public class HttpTestClient : ITestService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ITestServiceClientDef _client;

    public HttpTestClient(IServiceProvider services)
    {
        _httpClient = services.GetRequiredService<HttpClient>();
        _client = RestEaseBuilder.CreateRestClient(services, _httpClient).For<ITestServiceClientDef>();
    }

    public void Dispose()
        => _httpClient.Dispose();

    public Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => _client.SayHello(request, cancellationToken);

    public Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => _client.GetUser(userId, cancellationToken);

    public Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => _client.Sum(a, b, cancellationToken);
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
