namespace Samples.RpcBenchmark.Client;
using static Settings;

public class BenchmarkWorker
{
    public readonly Benchmark Benchmark;
    public readonly int Index;
    public readonly ITestService TestService;

    public BenchmarkWorker(Benchmark benchmark, ITestService testService, int index)
    {
        Benchmark = benchmark;
        Index = index;
        TestService = testService;
    }

    public virtual async Task<long> TestSayHello(
        Task<CpuTimestamp> whenReady, CancellationToken cancellationToken)
    {
        if (TestService is IHasWhenReady hasWhenReady)
            await hasWhenReady.WhenReady.WaitAsync(cancellationToken).ConfigureAwait(false);

        var count = 0L;
        var endsAt = await whenReady.ConfigureAwait(false);
        var request = HelloRequest.ExamplePayload;
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
}
