namespace Samples.RpcBenchmark.Client;

public class Benchmark
{
    public ClientCommand Command { get; }
    public string Title { get; }
    public bool IsGrpc { get; }
    public BenchmarkWorker[] Workers { get; }

    public Benchmark(ClientCommand command, string title, Func<ITestService> clientFactory)
    {
        Command = command;
        Title = title;
        var client = clientFactory.Invoke();
        IsGrpc = client is GrpcTestClient;
        var clientConcurrency = IsGrpc
            ? command.GrpcClientConcurrency
            : command.ClientConcurrency;
        Workers = new BenchmarkWorker[command.Workers];
        for (var i = 0; i < Workers.Length; i++) {
            if (i % clientConcurrency == 0 && i != 0)
                client = clientFactory.Invoke();
            Workers[i] = new BenchmarkWorker(this, client, i);
        }
    }

    public async Task Run()
    {
        WriteLine($"{Title}:");
        if (!IsGrpc) {
            await RunOne("Sum", (w, whenReady) => w.TestSum(whenReady)).ConfigureAwait(false);
            await RunOne("GetUser", (w, whenReady) => w.TestGetUser(whenReady)).ConfigureAwait(false);
            await RunOne("SayHello", (w, whenReady) => w.TestSayHello(whenReady)).ConfigureAwait(false);
        }
        else {
            await RunOne("Sum", (w, whenReady) => w.GrpcTestSum(whenReady)).ConfigureAwait(false);
            await RunOne("GetUser", (w, whenReady) => w.GrpcTestGetUser(whenReady)).ConfigureAwait(false);
            await RunOne("SayHello", (w, whenReady) => w.GrpcTestSayHello(whenReady)).ConfigureAwait(false);
        }

        // Dispose clients
        var clients = Workers.Select(w => w.Client).ToHashSet();
        foreach (var client in clients)
            if (client is IDisposable d)
                d.Dispose();
    }

    // Private methods

    private async Task RunOne(string name, BenchmarkTest benchmarkTest)
    {
        Write($"  {name,-9}: ");
        const int warmupCount = 3;
        for (var i = 0; i < warmupCount; i++) {
            await RunWorkers(benchmarkTest, Command.WarmupDuration / warmupCount).ConfigureAwait(false);
            GC.Collect();
        }

        var bestCps = 0d;
        for (var i = 0; i < Command.TryCount; i++) {
            var result = await RunWorkers(benchmarkTest, Command.Duration).ConfigureAwait(false);
            var cps = result.Count / (result.Duration / Workers.Length);
            Write($"{cps.FormatCount()} ");
            bestCps = Math.Max(bestCps, cps);
            GC.Collect();
        }
        WriteLine($"-> {bestCps.FormatCount()} calls/s");
    }

    private async Task<BenchmarkResult> RunWorkers(BenchmarkTest benchmarkTest, double duration)
    {
        var whenReadySource = TaskCompletionSourceExt.New<CpuTimestamp>();
        var runTask = Workers
            .Select(w => benchmarkTest.Invoke(w, whenReadySource.Task))
            .ToArray();
        whenReadySource.SetResult(CpuTimestamp.Now + TimeSpan.FromSeconds(duration));

        var result = new BenchmarkResult();
        foreach (var task in runTask)
            result += await task.ConfigureAwait(false);
        return result;
    }
}
