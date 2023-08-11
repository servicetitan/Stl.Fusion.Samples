using static System.Console;

namespace Samples.RpcBenchmark.Client;
using static Settings;

public class Benchmark
{
    public string Title { get; }
    public BenchmarkWorker[] Workers { get; }
    public bool IsGrpc { get; }

    public Benchmark(string title, Func<ITestService> testServiceFactory)
    {
        Title = title;
        Workers = new BenchmarkWorker[WorkerCount];
        var testService = testServiceFactory.Invoke();
        IsGrpc = testService is GrpcTestServiceClient;
        var clientConcurrency = IsGrpc ? GrpcClientConcurrency : ClientConcurrency;
        for (var i = 0; i < Workers.Length; i++) {
            if (i % clientConcurrency == 0 && i != 0)
                testService = testServiceFactory.Invoke();
            Workers[i] = new BenchmarkWorker(this, testService, i);
        }
    }

    public async Task Run(CancellationToken cancellationToken = default)
    {
        WriteLine($"{Title}:");
        if (!IsGrpc) {
            await RunTest("Sum", (w, whenReady) => w.TestSum(whenReady, cancellationToken));
            await RunTest("GetUser", (w, whenReady) => w.TestGetUser(whenReady, cancellationToken));
            await RunTest("SayHello", (w, whenReady) => w.TestSayHello(whenReady, cancellationToken));
        }
        else {
            await RunTest("Sum", (w, whenReady) => w.GrpcTestSum(whenReady, cancellationToken));
            await RunTest("GetUser", (w, whenReady) => w.GrpcTestGetUser(whenReady, cancellationToken));
            await RunTest("SayHello", (w, whenReady) => w.GrpcTestSayHello(whenReady, cancellationToken));
        }
    }

    // Private methods

    private async Task RunTest(string name, BenchmarkTest benchmarkTest)
    {
        Write($"  {name,-9}: ");
        await RunWorkers(benchmarkTest, WarmupDuration).ConfigureAwait(false);
        var count = await RunWorkers(benchmarkTest, Duration).ConfigureAwait(false);
        WriteLine(count.FormatRps(Duration));
    }

    private async Task<long> RunWorkers(BenchmarkTest benchmarkTest, TimeSpan duration)
    {
        if (ForceGCCollect) {
            GC.Collect();
            await Task.Delay(100).ConfigureAwait(false);
            GC.Collect();
        }

        var whenReadySource = TaskCompletionSourceExt.New<CpuTimestamp>();
        var runTask = Workers
            .Select(w => Task.Run(
                () => benchmarkTest.Invoke(w, whenReadySource.Task),
                CancellationToken.None))
            .ToList();
        whenReadySource.SetResult(CpuTimestamp.Now + duration);

        var count = 0L;
        foreach (var task in runTask)
            count += await task.ConfigureAwait(false);
        return count;
    }
}
