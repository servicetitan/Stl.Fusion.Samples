using System.ComponentModel;
using System.Security.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Samples.RpcBenchmark.Server;
using Stl.Rpc;
using Stl.Rpc.Server;

namespace Samples.RpcBenchmark;

[GeneratedParser]
[Command]
[Description("Starts the server part of this benchmark.")]
public partial class ServerCommand : BenchmarkCommandBase
{
    [CommandLineArgument(IsPositional = true, IsRequired = false)]
    [Description("The URL to bind to.")]
    public string Url { get; set; } = DefaultUrl;

    public override async Task<int> RunAsync()
    {
        Url = Url.NormalizeBaseUrl();
        SystemSettings.Apply(MinWorkerThreads, MinIOThreads, ByteSerializer);
        var cancellationToken = StopToken;
        WriteLine($"Starting server @ {Url}");

        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders()
            .AddDebug()
            .SetMinimumLevel(LogLevel.Warning);
            // .SetMinimumLevel(LogLevel.Debug)
            // .AddFilter("Microsoft", LogLevel.Information)
            // .AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);

        // Core services
        var services = builder.Services;
        services.AddSignalR(hub => {
            hub.MaximumParallelInvocationsPerClient = 1000; // Can't be too high, otherwise SignalR fails!
            hub.DisableImplicitFromServicesParameters = true;
        });
        var rpc = services.AddRpc();
        rpc.AddWebSocketServer();
        services.AddGrpc(o => o.IgnoreUnknownServices = true);
        services.AddMagicOnion();
        services.Configure<RouteOptions>(c => c.SuppressCheckForUnhandledSecurityMetadata = true);

        // Benchmark services
        services.AddSingleton<TestService>();
        services.AddSingleton<JsonRpcTestService>();
        rpc.AddServer<ITestService, TestService>();

        // Kestrel
        builder.WebHost.ConfigureKestrel(kestrel => {
            kestrel.AddServerHeader = false;
            kestrel.ConfigureEndpointDefaults(listen => {
                listen.Protocols = HttpProtocols.Http1 | HttpProtocols.Http2;
            });
            var limits = kestrel.Limits;
            limits.MaxConcurrentConnections = 20_000;
            limits.MaxConcurrentUpgradedConnections = 20_000;
            kestrel.AddServerHeader = false;
            kestrel.ConfigureHttpsDefaults(https => {
                https.AllowAnyClientCertificate();
                https.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                https.HandshakeTimeout = TimeSpan.FromSeconds(30);
            });
            var http2 = limits.Http2;
            http2.InitialConnectionWindowSize = 10 * 1024 * 1024;
            http2.InitialStreamWindowSize = 10 * 768 * 1024;
            http2.MaxStreamsPerConnection = 20_000;
        });
        var app = builder.Build();

        // Map services
        app.UseWebSockets();
        app.UseMiddleware<AppServicesMiddleware>();
        app.MapRpcWebSocketServer();
        // app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
        app.MapGrpcService<GrpcTestService>();
        app.MapHub<TestHub>("hubs/testService", o => {
            o.Transports = HttpTransportType.WebSockets;
            o.AllowStatefulReconnects = false;
        });
        app.MapMagicOnionService();
        app.MapStreamJsonRpcService<JsonRpcTestService>("stream-json-rpc");
        app.MapTestService<TestService>("/api/testService");
        app.Urls.Add(Url);
        try {
            await app.StartAsync(cancellationToken);
            await TaskExt.NeverEndingTask.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) { }
        catch (Exception error) {
            await Error.WriteLineAsync($"Server failed: {error.Message}");
            return 1;
        }
        return 0;
    }
}
