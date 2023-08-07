using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
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
    services.AddSingleton<TestService>();

    // Benchmark services
    rpc.AddServer<ITestService, TestService>();

    // Build app & initialize DB
    var app = builder.Build();

    // Start Kestrel
    app.Urls.Add(BaseUrl);
    app.UseWebSockets();
    app.MapRpcWebSocketServer();
    app.MapHub<TestHub>("hubs/testService", o => {
        o.Transports = HttpTransportType.WebSockets;
    });
    app.MapTestService<TestService>("/api/testService");
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
    WriteLine($"Service concurrency: {TestServiceConcurrency} workers per test service");

    // Run
    WriteLine();
    await new Benchmark("Stl.Rpc Client", ClientServices.RpcClientService).Run();
    await new Benchmark("SignalR Client", ClientServices.SignalRClientService).Run();
    await new Benchmark("RestEase (HTTP) Client", ClientServices.HttpClientService).Run();

    ReadKey();
    // ReSharper disable once AccessToDisposedClosure
    stopCts.Cancel();
}
