using System.ComponentModel;
using Ookii.CommandLine;
using Ookii.CommandLine.Commands;

namespace Samples.RpcBenchmark;

[GeneratedParser]
[Command]
[Description("Starts both the client and the server part of this benchmark.")]
public partial class TestCommand : ClientCommand
{
    public override Task<int> RunAsync()
    {
        SystemSettings.Apply(MinWorkerThreads, MinIOThreads, ByteSerializer);
        var serverCommand = new ServerCommand() { Url = Url };
        _ = serverCommand.RunAsync();
        return base.RunAsync();
    }
}
