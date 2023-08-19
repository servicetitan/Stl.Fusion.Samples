using Microsoft.AspNetCore.Http;
using StreamJsonRpc;

namespace Samples.RpcBenchmark.Server;

public static class StreamJsonRpcEndpoint
{
    public static async Task Invoke<TService>(TService service, HttpContext context)
        where TService : class
    {
        if (!context.WebSockets.IsWebSocketRequest) {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
        var webSocketMessageHandler = new WebSocketMessageHandler(webSocket);
        using var jsonRpc = new JsonRpc(webSocketMessageHandler);
        // See https://github.com/microsoft/vs-streamjsonrpc/blob/main/doc/resiliency.md#default-ordering-and-concurrency-behavior
        jsonRpc.SynchronizationContext = null;
        jsonRpc.AddLocalRpcTarget(service);
        jsonRpc.StartListening();
        await jsonRpc.Completion.ConfigureAwait(false);
    }
}
