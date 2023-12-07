using Stl.OS;

namespace Samples.RpcBenchmark.Client;

public class BenchmarkRunner : BenchmarkRunnerBase<double>
{
    private volatile Func<BenchmarkWorker, Func<CancellationToken, Task>> _currentOperationFactory = null!;

    public ClientCommand Command { get; }
    public bool IsStreaming { get; }
    public bool IsGrpc { get; }
    public BenchmarkWorker[] Workers { get; }

    public BenchmarkRunner(ClientCommand command, Func<ITestService> clientFactory, bool isStreaming)
    {
        Command = command;
        IsStreaming = isStreaming;
        TryCount = command.TryCount;
        TitleFormatter = title => $"  {title,-9}: ";
        ResultFormatter = x => $"{x.FormatCount(),7}";
        if (command.Benchmark == BenchmarkKind.Streams)
            Units = "items/s";
        var clientConcurrency = command.ClientConcurrencyValue;
        var workerCount = command.WorkersValue;

        var client = clientFactory.Invoke();
        IsGrpc = client is GrpcTestClient;
        Workers = new BenchmarkWorker[workerCount];
        for (var i = 0; i < Workers.Length; i++) {
            if (i % clientConcurrency == 0 && i != 0)
                client = clientFactory.Invoke();
            Workers[i] = IsGrpc
                ? new GrpcBenchmarkWorker(client)
                : new BenchmarkWorker(client);
        }
    }

    public async Task RunAll(string title, CancellationToken cancellationToken)
    {
        WriteLine($"{title}:");
        if (Command.Benchmark == BenchmarkKind.Calls) {
            await RunOne("Sum", w => w.Sum, cancellationToken);
            await RunOne("GetUser", w => w.GetUser, cancellationToken);
            await RunOne("SayHello", w => w.SayHello, cancellationToken);
        }
        else {
            await RunOne("StreamS", w => w.StreamS, cancellationToken);
            await RunOne("StreamL", w => w.StreamL, cancellationToken);
        }

        // Dispose clients
        var clients = Workers.Select(w => w.Client).ToHashSet();
        foreach (var client in clients)
            if (client is IDisposable d)
                d.Dispose();
        await Task.Delay(500).ConfigureAwait(false); // Wait when HTTP connections actually get closed
    }

    // Protected & private methods

    protected override async Task Warmup(CancellationToken cancellationToken)
    {
        const int partCount = 3;
        var partDuration = Command.WarmupDuration / partCount;
        for (var i = 0; i < partCount; i++)
            await GetCallFrequency(partDuration, cancellationToken).ConfigureAwait(false);
    }

    protected override async Task<double> Benchmark(CancellationToken cancellationToken)
    {
        var callFrequency = await GetCallFrequency(Command.Duration, cancellationToken).ConfigureAwait(false);
        if (Command.Benchmark == BenchmarkKind.Streams)
            callFrequency *= BenchmarkWorker.StreamLength;
        return callFrequency;
    }

    private Task<double> GetCallFrequency(double duration, CancellationToken cancellationToken)
        => Benchmarks.CallFrequency(Workers, duration, cancellationToken, _currentOperationFactory, w => w.WhenReady());

    private Task RunOne(
        string title,
        Func<BenchmarkWorker, Func<CancellationToken, Task>> operationFactory,
        CancellationToken cancellationToken)
    {
        Title = title;
        Interlocked.Exchange(ref _currentOperationFactory, operationFactory);
        return Run(cancellationToken);
    }
}
