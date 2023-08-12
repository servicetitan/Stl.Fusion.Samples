using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Stl.RestEase;
using Stl.Rpc;

namespace Samples.RpcBenchmark.Client;

public sealed class ClientFactories
{
    public readonly string BaseUrl;
    public readonly Func<ITestService> Rpc;
    public readonly Func<ITestService> SignalR;
    public readonly Func<ITestService> Grpc;
    public readonly Func<ITestService> Http;

    public (string Name, Func<ITestService> Factory) this[BenchmarkKind benchmarkKind]
        => benchmarkKind switch {
            BenchmarkKind.StlRpc => ("Stl.Rpc", Rpc),
            BenchmarkKind.SignalR => ("SignalR", SignalR),
            BenchmarkKind.Grpc => ("gRPC", Grpc),
            BenchmarkKind.Http => ("HTTP", Http),
            _ => throw new ArgumentOutOfRangeException(nameof(benchmarkKind), benchmarkKind, null)
        };

    public ClientFactories(string baseUrl)
    {
        BaseUrl = baseUrl;
        Rpc = CreateClientFactory<ITestService>();
        SignalR = CreateClientFactory<SignalRTestClient>();
        Grpc = CreateClientFactory<GrpcTestClient>();
        Http = CreateClientFactory<HttpTestClient>();
    }

    // Private methods

    private Func<ITestService> CreateClientFactory<TClient>()
        where TClient : class, ITestService
    {
        var services = CreateBaseServiceCollection();
        if (typeof(TClient) == typeof(ITestService))
            services.AddRpc().AddClient<ITestService>();
        else
            services.AddSingleton<TClient>();
        return () => {
            var c = services.BuildServiceProvider();
            return c.GetRequiredService<TClient>();
        };
    }

    private IServiceCollection CreateBaseServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddSingleton(this);

        // Rpc
        services.AddRpc().AddWebSocketClient(BaseUrl);

        // SignalR
        services.AddSingleton(_ => {
            var connection = new HubConnectionBuilder()
                .WithUrl($"{BaseUrl}hubs/testService")
                .Build();
            return connection;
        });

        // RestEase/HTTP
        var restEase = services.AddRestEase();
        var baseAddress = new Uri(BaseUrl);
        restEase.ConfigureHttpClient((_, name, o) => {
            o.HttpMessageHandlerBuilderActions.Add(h => h.PrimaryHandler = new HttpClientHandler() {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            });
            o.HttpClientActions.Add(c => {
                c.BaseAddress = baseAddress;
                c.DefaultRequestVersion = HttpVersion.Version20;
                c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
            });
        });
        return services;
    }
}
