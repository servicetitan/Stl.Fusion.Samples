using System.ComponentModel;
using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Validation;
using Samples.RpcBenchmark.Client;
using Stl.OS;

namespace Samples.RpcBenchmark;

[GeneratedParser]
[Command]
[Description("Starts the client part of this benchmark.")]
public partial class ClientCommand : BenchmarkCommandBase
{
    [CommandLineArgument]
    [Description("Benchmarks to run.")]
    [ValueDescription("Any subset of StlRpc,SignalR,Grpc,Http")]
    [ValidateRange(1, null)]
    [Alias("b")]
    public string Benchmarks { get; set; } = "StlRpc, SignalR, Grpc, Http";

    [CommandLineArgument]
    [Description("Client concurrency - the number of worker tasks using a single client.")]
    [ValueDescription("Number")]
    [ValidateRange(1, null)]
    [Alias("cc")]
    public int ClientConcurrency { get; set; } = 120;

    [CommandLineArgument("GrpcClientConcurrency")]
    [Description("Client concurrency for gRPC tests.")]
    [ValueDescription("Number")]
    [ValidateRange(1, null)]
    [Alias("grpc-cc")]
    public int? GrpcClientConcurrencyOverride { get; set; }

    public int GrpcClientConcurrency => GrpcClientConcurrencyOverride ?? ClientConcurrency;

    [CommandLineArgument]
    [Description("Worker count - the total number of worker tasks.")]
    [ValueDescription("Number")]
    [ValidateRange(1, null)]
    [Alias("w")]
    public int Workers { get; set; } = HardwareInfo.ProcessorCount * 300;

    [CommandLineArgument]
    [Description("Test duration in seconds.")]
    [ValueDescription("Number")]
    [ValidateRange(0.1d, null)]
    [Alias("d")]
    public double Duration { get; set; } = 5;

    [CommandLineArgument]
    [Description("Pre-test warmup duration in seconds.")]
    [ValidateRange(0.1d, null)]
    [Alias("wd")]
    public double WarmupDuration { get; set; } = 5;

    [CommandLineArgument]
    [Description("Test (attempt) count.")]
    [ValidateRange(0.1d, null)]
    [Alias("n")]
    public int TryCount { get; set; } = 5;

    [CommandLineArgument]
    [Description("Wait for a key press when benchmark ends.")]
    public bool Wait { get; set; }

    [CommandLineArgument(IsPositional = true, IsRequired = false)]
    [Description("The server URL to connect to.")]
    public string Url { get; set; } = DefaultUrl;

    public override async Task<int> RunAsync()
    {
        SystemSettings.Apply(this);
        var cancellationToken = StopToken;

        await ServerChecker.WhenReady(Url, cancellationToken);
        WriteLine("Client settings:");
        WriteLine($"  Server URL:           {Url}");
        WriteLine($"  Test plan:            {WarmupDuration:N}s warmup, {TryCount} x {Duration:N}s runs");
        WriteLine($"  Total worker count:   {Workers}");
        WriteLine($"  Client concurrency:   {ClientConcurrency}");
        WriteLine($"  Client count:         {Workers / ClientConcurrency}");
        if (GrpcClientConcurrency != ClientConcurrency) {
            WriteLine("Client settings for gRPC tests:");
            WriteLine($"  Client concurrency:   {GrpcClientConcurrency}");
            WriteLine($"  Client count:         {Workers / GrpcClientConcurrency}");
        }
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);

        // Run
        WriteLine();
        var clientFactories = new ClientFactories(Url);
        var benchmarkKinds = Benchmarks.Split(",").Select(x => Enum.Parse<BenchmarkKind>(x.Trim())).ToList();
        foreach (var benchmarkKind in benchmarkKinds) {
            var (name, factory) = clientFactories[benchmarkKind];
            await new Benchmark(this, $"{name} Client", factory).Run();
        }

        if (Wait)
            ReadKey();
        await StopTokenSource.CancelAsync(); // Stops the server if it's running
        return 0;
    }
}
