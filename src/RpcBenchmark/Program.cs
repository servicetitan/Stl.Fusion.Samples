using Ookii.CommandLine;
using Ookii.CommandLine.Commands;

namespace Samples.RpcBenchmark;

public static class Program
{
    public static readonly string DefaultUrl = "https://localhost:22444/";
    public static readonly CancellationTokenSource StopTokenSource = new();
    public static readonly CancellationToken StopToken = StopTokenSource.Token;

    public static async Task<int> Main(string[] args)
    {
        TreatControlCAsInput = false;
        CancelKeyPress += (_, e) => {
            StopTokenSource.Cancel();
            e.Cancel = true;
        };

        if (args.Length == 0)
            return await new TestCommand().RunAsync();

        var options = new CommandOptions() {
            ArgumentNamePrefixes = new [] { "-" },
            CommandNameTransform = NameTransform.DashCase,
            ArgumentNameTransform = NameTransform.DashCase,
            StripCommandNameSuffix = "Command",
        };
        var rootCommand = new RootCommand(options);
        return await rootCommand.RunCommandAsync() ?? 1;
    }
}
