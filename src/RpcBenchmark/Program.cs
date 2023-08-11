using System.Security.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Samples.RpcBenchmark;
using Samples.RpcBenchmark.Client;
using Samples.RpcBenchmark.Server;
using Stl.Rpc;
using Stl.Rpc.Server;
using static System.Console;
using static Samples.RpcBenchmark.Settings;

#pragma warning disable ASP0000

var minThreadCount = WorkerCount * 2;
ThreadPool.SetMinThreads(minThreadCount, minThreadCount);
ThreadPool.SetMaxThreads(16_384, 16_384);
ByteSerializer.Default = MessagePackByteSerializer.Default; // Remove to switch back to MemoryPack

using var stopCts = new CancellationTokenSource();
// ReSharper disable once AccessToDisposedClosure
CancelKeyPress += (s, ea) => stopCts.Cancel();
var cancellationToken = stopCts.Token;

await (args switch {
    [ "server" ] => RunServer(),
    [ "client" ] => RunClient(),
    _ => Task.WhenAll(RunServer(), RunClient()),
});

async Task RunServer()
{
    var builder = WebApplication.CreateBuilder();
    builder.Logging.ClearProviders()
        .AddDebug()
        .SetMinimumLevel(LogLevel.Warning);
        // .SetMinimumLevel(LogLevel.Debug)
        // .AddFilter("Microsoft", LogLevel.Information)
        // .AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);

    // Core services
    var services = builder.Services;
    services.AddSignalR();
    var rpc = services.AddRpc();
    rpc.AddWebSocketServer();
    services.AddGrpc(o => o.IgnoreUnknownServices = true);
    services.Configure<RouteOptions>(c => c.SuppressCheckForUnhandledSecurityMetadata = true);

    // Benchmark services
    services.AddSingleton<TestService>();
    rpc.AddServer<ITestService, TestService>();

    // Kestrel
    builder.WebHost.ConfigureKestrel(kestrel => {
        kestrel.AddServerHeader = false;
        kestrel.ConfigureHttpsDefaults(https => {
            https.SslProtocols = SslProtocols.Tls13;
        });
        var http2 = kestrel.Limits.Http2;
        http2.InitialConnectionWindowSize = 2 * 1024 * 1024;
        http2.InitialStreamWindowSize = 1024 * 1024;
        http2.MaxStreamsPerConnection = 16_000;
    });
    var app = builder.Build();

    //Map services there
    app.UseWebSockets();
    app.UseMiddleware<AppServicesMiddleware>();
    app.MapRpcWebSocketServer();
    app.MapGrpcService<GrpcTestService>();
    app.MapHub<TestHub>("hubs/testService", o => {
        o.Transports = HttpTransportType.WebSockets;
    });
    app.MapTestService<TestService>("/api/testService");
    app.Urls.Add(BaseUrl);
    try {
        await app.StartAsync(cancellationToken);
        await TaskExt.NeverEndingTask.WaitAsync(cancellationToken);
    }
    catch (OperationCanceledException) { }
    catch (Exception error) {
        Error.WriteLine($"Server failed: {error.Message}");
    }
}

async Task RunClient()
{
    // Initialize
    await ServerChecker.WhenReady(BaseUrl, cancellationToken);
    WriteLine("Settings (default / gRPC):");
    WriteLine($"  Total worker count: {WorkerCount} / {GrpcWorkerCount}");
    WriteLine($"  Client concurrency: {ClientConcurrency} / {GrpcClientConcurrency}");
    WriteLine($"  Client count:       {WorkerCount / ClientConcurrency} / {GrpcWorkerCount / GrpcClientConcurrency}");

    // Run
    WriteLine();
    await new Benchmark("Stl.Rpc Client", ClientServices.RpcClientService).Run();
    await new Benchmark("SignalR Client", ClientServices.SignalRClientService).Run();
    await new Benchmark("gRPC Client", ClientServices.GrpcClientService).Run();
    await new Benchmark("RestEase (HTTP) Client", ClientServices.HttpClientService).Run();

    ReadKey();
    // ReSharper disable once AccessToDisposedClosure
    stopCts.Cancel();
}
