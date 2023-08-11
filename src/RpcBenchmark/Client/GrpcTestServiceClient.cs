using System.Net.Http;
using Grpc.Net.Client;

namespace Samples.RpcBenchmark.Client;

public class GrpcTestServiceClient : ITestService
{
    public readonly GrpcService.GrpcServiceClient Client;

    public GrpcTestServiceClient(IServiceProvider services)
    {
        var httpClient = services.GetRequiredService<HttpClient>();
        var channelOptions = new GrpcChannelOptions() { HttpClient = httpClient };
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
