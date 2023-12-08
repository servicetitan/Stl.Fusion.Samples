using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.WebSockets;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Stl.RestEase;
using Stl.Rpc;
using Stl.Rpc.Clients;
using Stl.Rpc.Infrastructure;
using Stl.Rpc.WebSockets;

namespace Samples.RpcBenchmark.Client;

public sealed class ClientFactories
{
    public readonly string BaseUrl;
    public readonly Func<ITestService> Rpc;
    public readonly Func<ITestService> SignalR;
    public readonly Func<ITestService> StreamJsonRpc;
    public readonly Func<ITestService> MagicOnion;
    public readonly Func<ITestService> Grpc;
    public readonly Func<ITestService> Http;

    public (string Name, Func<ITestService> Factory) this[LibraryKind libraryKind]
        => libraryKind switch {
            LibraryKind.StlRpc => ("Stl.Rpc", Rpc),
            LibraryKind.SignalR => ("SignalR", SignalR),
            LibraryKind.StreamJsonRpc => ("StreamJsonRpc", StreamJsonRpc),
            LibraryKind.MagicOnion => ("MagicOnion", MagicOnion),
            LibraryKind.Grpc => ("gRPC", Grpc),
            LibraryKind.Http => ("HTTP", Http),
            _ => throw new ArgumentOutOfRangeException(nameof(libraryKind), libraryKind, null)
        };

    public ClientFactories(string baseUrl)
    {
        BaseUrl = baseUrl;
        Rpc = CreateClientFactory<ITestService>();
        SignalR = CreateClientFactory<SignalRTestClient>();
        StreamJsonRpc = CreateClientFactory<StreamJsonRpcTestClient>();
        MagicOnion = CreateClientFactory<MagicOnionTestClient>();
        Grpc = CreateClientFactory<GrpcTestClient>();
        Http = CreateClientFactory<HttpTestClient>();
    }

    // Private methods

    private Func<ITestService> CreateClientFactory<TClient>()
        where TClient : class, ITestService
    {
        var services = CreateBaseServiceCollection();
#if false
        services.AddLogging(logging => logging
            .AddDebug()
            .SetMinimumLevel(LogLevel.Debug)
            .AddFilter("Microsoft", LogLevel.Debug)
        );
#endif

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
        services.AddRpc().AddWebSocketClient(c => RpcWebSocketClient.Options.Default with {
            HostUrlResolver = (_, _) => BaseUrl,
            WebSocketChannelOptions = WebSocketChannel<RpcMessage>.Options.Default with {
                WriteFrameSize = 4350,
            },
            WebSocketOwnerFactory = (_, peer) => {
                var ws = new ClientWebSocket();
                ws.Options.HttpVersion = HttpVersion.Version11;
                ws.Options.HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                ws.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
                return new WebSocketOwner(peer.Ref.Key, ws, c);
            },
        });

        // SignalR
        services.AddSingleton(c => {
            var connection = new HubConnectionBuilder()
                .WithUrl($"{BaseUrl}hubs/testService", options => {
                    options.Transports = HttpTransportType.WebSockets;
                    options.HttpMessageHandlerFactory = _ => new SocketsHttpHandler() {
                        PooledConnectionLifetime = TimeSpan.FromDays(1),
                        EnableMultipleHttp2Connections = true,
                        MaxConnectionsPerServer = 20_000,
                        SslOptions = new SslClientAuthenticationOptions() {
                            RemoteCertificateValidationCallback = (_, _, _, _) => true,
                        },
                    };
                    options.WebSocketConfiguration = ws => {
                        ws.HttpVersion = HttpVersion.Version11;
                        ws.HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                        ws.RemoteCertificateValidationCallback = (_, _, _, _) => true;
                    };
                })
                .Build();
            return connection;
        });

        // gRPC
        services.AddSingleton(c => {
            var channelOptions = new GrpcChannelOptions() {
                // HttpClient = httpClient,
                HttpHandler = new SocketsHttpHandler {
                    PooledConnectionLifetime = TimeSpan.FromDays(1),
                    EnableMultipleHttp2Connections = true,
                    MaxConnectionsPerServer = 20_000,
                    SslOptions = new SslClientAuthenticationOptions() {
                        RemoteCertificateValidationCallback = (_, _, _, _) => true,
                    },
                }
            };
            return GrpcChannel.ForAddress(BaseUrl, channelOptions);
        });

        // StreamJsonRpc
        services.AddTransient(c => {
            var ws = new ClientWebSocket();
            ws.Options.HttpVersion = HttpVersion.Version11;
            ws.Options.HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact;
            ws.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
            return ws;
        });

        // RestEase/HTTP
        var baseAddress = new Uri(BaseUrl);
        services.AddRestEase();
        services.AddSingleton(c => {
            var handler = new SocketsHttpHandler() {
                PooledConnectionLifetime = TimeSpan.FromDays(1),
                EnableMultipleHttp2Connections = true,
                MaxConnectionsPerServer = 20_000,
                SslOptions = new SslClientAuthenticationOptions() {
                    RemoteCertificateValidationCallback = (_, _, _, _) => true,
                },
            };
            var hc = new HttpClient(handler);
            hc.BaseAddress = baseAddress;
            hc.DefaultRequestVersion = HttpVersion.Version20;
            hc.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
            var restEaseClient = RestEaseBuilder.CreateRestClient(c, hc).For<ITestServiceClientDef>();
            return restEaseClient;
        });

        return services;
    }
}
