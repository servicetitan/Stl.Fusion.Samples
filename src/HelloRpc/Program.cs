using Microsoft.AspNetCore.Builder;
using Samples.HelloRpc;
using Stl.Rpc;
using Stl.Rpc.Server;
using static System.Console;

#pragma warning disable ASP0000

var baseUrl = "http://localhost:22222/";
await (args switch {
    [ "server" ] => RunServer(),
    [ "client" ] => RunClient(),
    _ => Task.WhenAll(RunServer(), RunClient()),
});

async Task RunServer()
{
    var builder = WebApplication.CreateBuilder();
    builder.Logging.ClearProviders().AddDebug();
    var rpc = builder.Services.AddRpc();
    rpc.AddWebSocketServer();
    rpc.AddServer<IGreeter, Greeter>();
    var app = builder.Build();

    app.UseWebSockets();
    app.MapRpcWebSocketServer();
    try {
        await app.RunAsync(baseUrl);
    }
    catch (Exception error) {
        Error.WriteLine($"Server failed: {error.Message}");
    }
}

async Task RunClient()
{
    var services = new ServiceCollection();
    var rpc = services.AddRpc();
    rpc.AddWebSocketClient(baseUrl);
    rpc.AddClient<IGreeter>();
    var serviceProvider = services.BuildServiceProvider();

    var greeter = serviceProvider.GetRequiredService<IGreeter>();
    while (true) {
        Write("Your name: ");
        var name = ReadLine() ?? "";
        try {
            var message = await greeter.SayHello(name);
            WriteLine(message);
        }
        catch (Exception error) {
            Error.WriteLine($"Error: {error.Message}");
        }
    }
}
