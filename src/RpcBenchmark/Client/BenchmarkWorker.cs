namespace Samples.RpcBenchmark.Client;
using static Settings;

public class BenchmarkWorker(Benchmark benchmark, ITestService testService, int index)
{
    public readonly Benchmark Benchmark = benchmark;
    public readonly int Index = index;
    public readonly ITestService TestService = testService;

    // Standard variants

    public virtual async Task<long> TestSayHello(
        Task<CpuTimestamp> whenReady, CancellationToken cancellationToken)
    {
        if (TestService is IHasWhenReady hasWhenReady)
            await hasWhenReady.WhenReady.WaitAsync(cancellationToken).ConfigureAwait(false);

        var count = 0L;
        var endsAt = await whenReady.ConfigureAwait(false);
        var request = Examples.HelloRequest;
        while ((count & TimeCheckCountMask) != 0 || CpuTimestamp.Now < endsAt) {
            var result = await TestService.SayHello(request, cancellationToken).ConfigureAwait(false);
            if (result.Response.Int32 != 32)
                throw new InvalidOperationException("Wrong result.");
            count++;
        }
        return count;
    }

    public virtual async Task<long> TestGetUser(
        Task<CpuTimestamp> whenReady, CancellationToken cancellationToken)
    {
        if (TestService is IHasWhenReady hasWhenReady)
            await hasWhenReady.WhenReady.WaitAsync(cancellationToken).ConfigureAwait(false);

        var count = 0L;
        var endsAt = await whenReady.ConfigureAwait(false);
        while ((count & TimeCheckCountMask) != 0 || CpuTimestamp.Now < endsAt) {
            var user = await TestService.GetUser(1, cancellationToken).ConfigureAwait(false);
            if (user!.Id != 1)
                throw new InvalidOperationException("Wrong result.");
            count++;
        }
        return count;
    }

    public virtual async Task<long> TestSum(
        Task<CpuTimestamp> whenReady, CancellationToken cancellationToken)
    {
        if (TestService is IHasWhenReady hasWhenReady)
            await hasWhenReady.WhenReady.WaitAsync(cancellationToken).ConfigureAwait(false);

        var count = 0L;
        var endsAt = await whenReady.ConfigureAwait(false);
        while ((count & TimeCheckCountMask) != 0 || CpuTimestamp.Now < endsAt) {
            var sum = await TestService.Sum(1, 2, cancellationToken).ConfigureAwait(false);
            if (sum != 3)
                throw new InvalidOperationException("Wrong result.");
            count++;
        }
        return count;
    }

    // GRPC variants

    public virtual async Task<long> GrpcTestSayHello(
        Task<CpuTimestamp> whenReady, CancellationToken cancellationToken)
    {
        var client = ((GrpcTestServiceClient)testService).Client;
        var count = 0L;
        var endsAt = await whenReady.ConfigureAwait(false);
        var request = Examples.GrpcHelloRequest;
        while ((count & TimeCheckCountMask) != 0 || CpuTimestamp.Now < endsAt) {
            var result = await client.SayHelloAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (result.Response.Int32 != 32)
                throw new InvalidOperationException("Wrong result.");
            count++;
        }
        return count;
    }

    public virtual async Task<long> GrpcTestGetUser(
        Task<CpuTimestamp> whenReady, CancellationToken cancellationToken)
    {
        var client = ((GrpcTestServiceClient)testService).Client;
        var count = 0L;
        var endsAt = await whenReady.ConfigureAwait(false);
        while ((count & TimeCheckCountMask) != 0 || CpuTimestamp.Now < endsAt) {
            var reply = await client
                .GetUserAsync(new GrpcGetUserRequest() { UserId = 1 }, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (reply.User.Id != 1)
                throw new InvalidOperationException("Wrong result.");
            count++;
        }
        return count;
    }

    public virtual async Task<long> GrpcTestSum(
        Task<CpuTimestamp> whenReady, CancellationToken cancellationToken)
    {
        var client = ((GrpcTestServiceClient)testService).Client;
        var count = 0L;
        var endsAt = await whenReady.ConfigureAwait(false);
        while ((count & TimeCheckCountMask) != 0 || CpuTimestamp.Now < endsAt) {
            var result = await client.SumAsync(new GrpcSumRequest { A = 1, B = 2 }, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (result.Sum != 3)
                throw new InvalidOperationException("Wrong result.");
            count++;
        }
        return count;
    }
}
