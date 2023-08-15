using Microsoft.AspNetCore.Http;
using StreamJsonRpc;

namespace Samples.RpcBenchmark.Server;

public static class StreamJsonRpcEndpoint
{
    public static async Task Invoke<TService>(TService service, HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest) {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
        var webSocketMessageHandler = new WebSocketMessageHandler(webSocket);
        using var jsonRpc = new JsonRpc(webSocketMessageHandler, service);
        jsonRpc.StartListening();
        await jsonRpc.Completion.ConfigureAwait(false);
    }
}
