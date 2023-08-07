using Microsoft.AspNetCore.Builder;
using Samples.Benchmark;
using Samples.Benchmark.Client;
using Samples.Benchmark.Server;
using Stl.Fusion.Server;
using Stl.Rpc;
using Stl.Rpc.Server;
using static System.Console;
using static Samples.Benchmark.Settings;

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
    builder.Logging.ClearProviders().AddDebug();

    // Core services
    var services = builder.Services;
    services.AddAppDbContext();
    var fusion = services.AddFusion(RpcServiceMode.Server);
    fusion.AddWebServer();

    // Benchmark services
    fusion.AddService<IFusionTestService, FusionTestService>();
    fusion.Rpc.AddServer<IRpcTestService, IFusionTestService>();

    // Build app & initialize DB
    var app = builder.Build();
    var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
    await dbInitializer.Initialize(true);

    // Start Kestrel
    app.Urls.Add(BaseUrl);
    app.UseWebSockets();
    app.MapRpcWebSocketServer();
    app.MapTestService<DbTestService>("/api/dbTestService");
    app.MapTestService<IFusionTestService>("/api/fusionTestService");
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
    var dbServices = ClientServices.DbServices;
    await ServerChecker.WhenReady(BaseUrl, cancellationToken);
    await dbServices.GetRequiredService<DbInitializer>().Initialize(true, cancellationToken);
    WriteLine($"Item count:          {ItemCount}");
    WriteLine($"Service concurrency: {TestServiceConcurrency} workers per test service");
    if (WriterFrequency is { } writerFrequency)
        WriteLine($"Writing worker %:    {1.0/writerFrequency:P}");
    var benchmark = new Benchmark("Initialize", ClientServices.LocalDbServiceFactory);
    await benchmark.Initialize(cancellationToken);

    // Run
    int m1 = 10;
    int m2 = 40;
    WriteLine();
    WriteLine("Local services:");
    await new Benchmark("Compute Service", ClientServices.LocalFusionServiceFactory).Run();
    await new Benchmark("Regular Service", ClientServices.LocalDbServiceFactory, m2).Run();

    WriteLine();
    WriteLine("Remote services:");
    await new Benchmark("Compute Service Client -> WebSocket -> Compute Service", ClientServices.RemoteFusionServiceFactory).Run();
    await new Benchmark("Stl.Rpc Client -> WebSocket -> Compute Service", ClientServices.RemoteFusionServiceViaRpcFactory, m2).Run();
    await new Benchmark("RestEase Client -> HTTP -> Compute Service", ClientServices.RemoteFusionServiceViaHttpFactory, m1).Run();
    await new Benchmark("RestEase Client -> HTTP -> Regular Service", ClientServices.RemoteDbServiceViaHttpFactory, m1).Run();

    ReadKey();
    // ReSharper disable once AccessToDisposedClosure
    stopCts.Cancel();
}
