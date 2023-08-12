using System.Runtime.CompilerServices;

namespace Samples.RpcBenchmark.Client;

public sealed class BenchmarkWorker(Benchmark benchmark, ITestService client, int index)
{
    private const long CancellationTokenRenewMask = 31;

    public readonly Benchmark Benchmark = benchmark;
    public readonly int Index = index;
    public readonly ITestService Client = client;

    // Standard variants

    public Task<BenchmarkResult> TestSayHello(Task<CpuTimestamp> whenReady)
        => Measure(whenReady, static async (client, cancellationToken) => {
            var result = await client.SayHello(Examples.HelloRequest, cancellationToken).ConfigureAwait(false);
            if (result.Response.Int32 != 32)
                throw new InvalidOperationException("Wrong result.");
        });

    public Task<BenchmarkResult> TestGetUser(Task<CpuTimestamp> whenReady)
        => Measure(whenReady, static async (client, cancellationToken) => {
            var user = await client.GetUser(1, cancellationToken).ConfigureAwait(false);
            if (user!.Id != 1)
                throw new InvalidOperationException("Wrong result.");
        });

    public Task<BenchmarkResult> TestSum(Task<CpuTimestamp> whenReady)
        => Measure(whenReady, static async (client, cancellationToken) => {
            var sum = await client.Sum(1, 2, cancellationToken).ConfigureAwait(false);
            if (sum != 3)
                throw new InvalidOperationException("Wrong result.");
        });

    // GRPC variants

    public Task<BenchmarkResult> GrpcTestSayHello(Task<CpuTimestamp> whenReady)
        => Measure(whenReady, static async (client, cancellationToken) => {
            var result = await client
                .SayHelloAsync(Examples.GrpcHelloRequest, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (result.Response.Int32 != 32)
                throw new InvalidOperationException("Wrong result.");
        });

    public Task<BenchmarkResult> GrpcTestGetUser(Task<CpuTimestamp> whenReady)
        => Measure(whenReady, static async (client, cancellationToken) => {
            var reply = await client
                .GetUserAsync(new GrpcGetUserRequest() { UserId = 1 }, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (reply.User.Id != 1)
                throw new InvalidOperationException("Wrong result.");
        });

    public Task<BenchmarkResult> GrpcTestSum(Task<CpuTimestamp> whenReady)
        => Measure(whenReady, static async (client, cancellationToken) => {
            var result = await client
                .SumAsync(new GrpcSumRequest { A = 1, B = 2 }, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (result.Sum != 3)
                throw new InvalidOperationException("Wrong result.");
        });

    // Private methods

    private async Task<BenchmarkResult> Measure(
        Task<CpuTimestamp> whenReady,
        Func<ITestService, CancellationToken, Task> oneIteration)
    {
        var stopToken = StopToken;
        if (Client is IHasWhenReady hasWhenReady)
            await hasWhenReady.WhenReady.WaitAsync(stopToken).ConfigureAwait(false);

        var client = Client;
        var cts = (CancellationTokenSource?)null;
        var endsAt = await whenReady.ConfigureAwait(false);
        var count = 0L;
        var now = CpuTimestamp.Now;
        var startedAt = now;
        while (now < endsAt) {
            if ((count & CancellationTokenRenewMask) == 0) {
                cts?.Dispose();
                cts = stopToken.CreateLinkedTokenSource();
            }
            await oneIteration.Invoke(client, cts!.Token).ConfigureAwait(false);
            count++;
            now = CpuTimestamp.Now;
        }
        cts?.Dispose();
        return new(count, (now - startedAt).TotalSeconds);
    }

    private async Task<BenchmarkResult> Measure(
        Task<CpuTimestamp> whenReady,
        Func<GrpcService.GrpcServiceClient, CancellationToken, Task> oneIteration)
    {
        var stopToken = StopToken;
        if (Client is IHasWhenReady hasWhenReady)
            await hasWhenReady.WhenReady.WaitAsync(stopToken).ConfigureAwait(false);

        var client = ((GrpcTestClient)Client).Client;
        var cts = (CancellationTokenSource?)null;
        var endsAt = await whenReady.ConfigureAwait(false);
        var count = 0L;
        var now = CpuTimestamp.Now;
        var startedAt = now;
        while (now < endsAt) {
            if ((count & CancellationTokenRenewMask) == 0) {
                cts?.Dispose();
                cts = stopToken.CreateLinkedTokenSource();
            }
            await oneIteration.Invoke(client, cts!.Token).ConfigureAwait(false);
            count++;
            now = CpuTimestamp.Now;
        }
        cts?.Dispose();
        return new(count, (now - startedAt).TotalSeconds);
    }
}
