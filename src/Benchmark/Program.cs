using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Samples.Benchmark;
using Samples.Benchmark.Client;
using Samples.Benchmark.Server;
using Stl.Fusion.Server;
using Stl.OS;
using Stl.Rpc;
using Stl.Rpc.Server;
using static System.Console;

#pragma warning disable ASP0000

using var cts = new CancellationTokenSource();
// ReSharper disable once AccessToDisposedClosure
CancelKeyPress += (s, ea) => cts.Cancel();
var cancellationToken = cts.Token;

await (args switch {
    [ "server" ] => RunServer(),
    [ "client" ] => RunClient(),
    _ => Task.WhenAll(RunServer(), RunClient()),
});

async Task RunServer()
{
    var builder = WebApplication.CreateBuilder();
    builder.Logging.ClearProviders().AddDebug();
    var services = builder.Services;
    services.AddAppDbContext();
    var fusion = services.AddFusion(RpcServiceMode.Server);

    // Benchmark services
    fusion.AddService<IFusionTenants, FusionTenants>();
    fusion.Rpc.AddServer<IRpcTenants, IFusionTenants>();

    // Fusion/RPC server + ASP.NET Core Controllers
    fusion.AddWebServer();
    services.AddMvc().AddApplicationPart(Assembly.GetExecutingAssembly());

    // Build app & initialize DB
    var app = builder.Build();
    var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
    await dbInitializer.Initialize(true);

    // Start Kestrel
    app.UseWebSockets();
    app.MapRpcWebSocketServer();
    app.MapControllers();
    try {
        await app.RunAsync(Settings.BaseUrl);
    }
    catch (Exception error) {
        Error.WriteLine($"Server failed: {error.Message}");
    }
}

async Task RunClient()
{
    // Initialize
    var dbServices = ClientServices.DbServices;
    await ServerChecker.WhenReady(Settings.BaseUrl, cancellationToken);
    await dbServices.GetRequiredService<DbInitializer>().Initialize(true, cancellationToken);
    var benchmark = new Benchmark() {
        TimeCheckOperationIndexMask = 7,
        ConcurrencyLevel = HardwareInfo.ProcessorCount * 8,
    };
    await benchmark.Initialize(cancellationToken);
    benchmark.DumpParameters();
    WriteLine();

    // Run
    WriteLine("Local services:");
    benchmark.TenantsFactory = ClientServices.LocalFusionTenantsFactory;
    await benchmark.Run("Compute Service", cancellationToken);
    benchmark.TenantsFactory = ClientServices.LocalDbTenantsFactory;
    await benchmark.Run("Regular Service", cancellationToken);

    WriteLine();
    WriteLine("Remote services:");
    benchmark.TenantsFactory = ClientServices.FusionClientToFusionTenantsFactory;
    await benchmark.Run("Compute Service Client -> Stl.Rpc -> Compute Service", cancellationToken);
    benchmark.TenantsFactory = ClientServices.RpcClientToFusionTenantsFactory;
    await benchmark.Run("Stl.Rpc Client -> Stl.Rpc -> Compute Service", cancellationToken);
    benchmark.TenantsFactory = ClientServices.HttpClientToFusionTenantsFactory;
    await benchmark.Run("RestEase Client -> HTTP -> Compute Service", cancellationToken);
    benchmark.TenantsFactory = ClientServices.HttpClientToDbTenantsFactory;
    await benchmark.Run("RestEase Client -> HTTP -> Regular Service", cancellationToken);
}
