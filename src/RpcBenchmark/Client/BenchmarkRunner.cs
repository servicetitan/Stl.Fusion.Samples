namespace Samples.RpcBenchmark.Client;

public class BenchmarkRunner : BenchmarkRunnerBase<double>
{
    private volatile Func<BenchmarkWorker, Func<CancellationToken, Task>> _currentOperationFactory = null!;

    public ClientCommand Command { get; }
    public bool IsGrpc { get; }
    public BenchmarkWorker[] Workers { get; }

    public BenchmarkRunner(ClientCommand command, Func<ITestService> clientFactory)
    {
        Command = command;
        TryCount = command.TryCount;
        TitleFormatter = title => $"  {title,-9}: ";
        ResultFormatter = x => $"{x.FormatCount(),7}";

        var clientConcurrency = command.ClientConcurrency;
        var client = clientFactory.Invoke();
        IsGrpc = client is GrpcTestClient;
        Workers = new BenchmarkWorker[command.Workers];
        for (var i = 0; i < Workers.Length; i++) {
            if (i % clientConcurrency == 0 && i != 0)
                client = clientFactory.Invoke();
            Workers[i] = IsGrpc
                ? new GrpcBenchmarkWorker(client)
                : new BenchmarkWorker(client);
        }
    }

    public async Task RunAll(string title)
    {
        WriteLine($"{title}:");
        await RunOne("Sum", w => w.Sum);
        await RunOne("GetUser", w => w.GetUser);
        await RunOne("SayHello", w => w.SayHello);

        // Dispose clients
        var clients = Workers.Select(w => w.Client).ToHashSet();
        foreach (var client in clients)
            if (client is IDisposable d)
                d.Dispose();
    }

    // Protected & private methods

    protected override async Task Warmup(CancellationToken cancellationToken)
    {
        const int partCount = 3;
        var partDuration = Command.WarmupDuration / partCount;
        for (var i = 0; i < partCount; i++)
            await GetCallFrequency(partDuration, cancellationToken).ConfigureAwait(false);
    }

    protected override Task<double> Benchmark(CancellationToken cancellationToken)
        => GetCallFrequency(Command.Duration, cancellationToken);

    private Task<double> GetCallFrequency(double duration, CancellationToken cancellationToken)
        => Benchmarks.CallFrequency(Workers, duration, cancellationToken, _currentOperationFactory, w => w.WhenReady());

    private Task RunOne(string title, Func<BenchmarkWorker, Func<CancellationToken, Task>> operationFactory)
    {
        Title = title;
        Interlocked.Exchange(ref _currentOperationFactory, operationFactory);
        return Run();
    }
}
