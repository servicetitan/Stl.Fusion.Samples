using System.ComponentModel;
using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Validation;
using Samples.RpcBenchmark.Client;
using Stl.OS;

namespace Samples.RpcBenchmark;

#pragma warning disable VSTHRD103

[GeneratedParser]
[Command]
[Description("Starts the client part of this benchmark.")]
public partial class ClientCommand : BenchmarkCommandBase
{
    [CommandLineArgument]
    [Description("Benchmark kind to run.")]
    [ValueDescription("Calls or Streams")]
    [Alias("b")]
    public BenchmarkKind Benchmark { get; set; }

    [CommandLineArgument]
    [Description("Libraries/APIs to benchmark.")]
    [ValueDescription("Any subset of StlRpc, SignalR, Grpc, MagicOnion, StreamJsonRpc, Http")]
    [ValidateRange(1, null)]
    [Alias("l")]
    public string Libraries { get; set; } = "StlRpc, SignalR, Grpc, MagicOnion, StreamJsonRpc, Http";

    [CommandLineArgument]
    [Description("Client concurrency - the number of worker tasks using a single client.")]
    [ValueDescription("Number")]
    [ValidateRange(1, null)]
    [Alias("cc")]
    public int? ClientConcurrency { get; set; }
    public int ClientConcurrencyValue => ClientConcurrency ?? (Benchmark == BenchmarkKind.Calls ? 100 : 4);

    [CommandLineArgument]
    [Description("Worker count - the total number of worker tasks.")]
    [ValueDescription("Number")]
    [ValidateRange(1, null)]
    [Alias("w")]
    public int? Workers { get; set; }
    public int WorkersValue => Workers ?? (int)(HardwareInfo.ProcessorCount * (Benchmark == BenchmarkKind.Calls ? 300 : 4));

    [CommandLineArgument]
    [Description("Test duration in seconds.")]
    [ValueDescription("Number")]
    [ValidateRange(0.1d, null)]
    [Alias("d")]
    public double Duration { get; set; } = 5;

    [CommandLineArgument]
    [Description("Pre-test warmup duration in seconds.")]
    [ValidateRange(0d, null)]
    [Alias("wd")]
    public double WarmupDuration { get; set; } = 5;

    [CommandLineArgument]
    [Description("Test (attempt) count.")]
    [ValidateRange(1, null)]
    [Alias("n")]
    public int TryCount { get; set; } = 4;

    [CommandLineArgument]
    [Description("Wait for a key press when benchmark ends.")]
    public bool Wait { get; set; }

    [CommandLineArgument(IsPositional = true, IsRequired = false)]
    [Description("The server URL to connect to.")]
    public string Url { get; set; } = DefaultUrl;

    public override async Task<int> RunAsync()
    {
        Url = Url.NormalizeBaseUrl();
        SystemSettings.Apply(MinWorkerThreads, MinIOThreads, ByteSerializer);
        var cancellationToken = StopToken;

        await TcpProbe.WhenReady(Url, cancellationToken);
        WriteLine("Client settings:");
        WriteLine($"  Server URL:           {Url}");
        WriteLine($"  Test plan:            {WarmupDuration:N}s warmup, {TryCount} x {Duration:N}s runs");
        WriteLine($"  Client count:         {(WorkersValue + ClientConcurrencyValue - 1) / ClientConcurrencyValue}");
        WriteLine($"  Client concurrency:   {ClientConcurrencyValue}");
        WriteLine($"  Total worker count:   {WorkersValue}");
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken); // Let server to complete the startup

        // Run
        WriteLine();
        var clientFactories = new ClientFactories(Url);
        var benchmarkKinds = Libraries.Split(",").Select(LibraryKindExt.Parse).ToArray();
        foreach (var benchmarkKind in benchmarkKinds) {
            var (name, factory) = clientFactories[benchmarkKind];
            await new BenchmarkRunner(this, factory, name.Contains("Stream")).RunAll(name, cancellationToken);
        }

        if (Wait)
            ReadKey();
        // ReSharper disable once MethodHasAsyncOverload
        StopTokenSource.Cancel(); // Stops the server if it's running
        return 0;
    }
}
