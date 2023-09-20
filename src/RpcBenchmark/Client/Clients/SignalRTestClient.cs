using Microsoft.AspNetCore.SignalR.Client;
using Stl.Rpc;

namespace Samples.RpcBenchmark.Client;

public class SignalRTestClient(HubConnection connection) : ITestService, IHasWhenReady
{
    public Task WhenReady { get; } = connection.State == HubConnectionState.Disconnected
        ? connection.StartAsync()
        : Task.CompletedTask;

    public Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        // SignalR currently doesn't support passing CancellationToken for non-streaming calls:
        // - https://github.com/dotnet/aspnetcore/issues/11542
        // ReSharper disable once MethodSupportsCancellation
        => connection.InvokeAsync<HelloReply>(nameof(SayHello), request);

    public Task<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        // SignalR currently doesn't support passing CancellationToken for non-streaming calls:
        // - https://github.com/dotnet/aspnetcore/issues/11542
        // ReSharper disable once MethodSupportsCancellation
        => connection.InvokeAsync<User?>(nameof(GetUser), userId);

    public Task<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        // SignalR currently doesn't support passing CancellationToken for non-streaming calls:
        // - https://github.com/dotnet/aspnetcore/issues/11542
        // ReSharper disable once MethodSupportsCancellation
        => connection.InvokeAsync<int>(nameof(Sum), a, b);

    public Task<RpcStream<Item>> GetItems(GetItemsRequest request, CancellationToken cancellationToken = default)
    {
        var items = connection.StreamAsync<Item>(nameof(GetItems), request, cancellationToken);
        return Task.FromResult(new RpcStream<Item>(items));
    }
}
