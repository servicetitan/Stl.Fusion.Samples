using System.ComponentModel;
using Ookii.CommandLine;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Validation;
using Stl.OS;

namespace Samples.RpcBenchmark;

public abstract class BenchmarkCommandBase : AsyncCommandBase
{
    [CommandLineArgument]
    [Description("Min. thread pool thread count.")]
    [ValueDescription("Number")]
    [ValidateRange(1, null)]
    [Alias("wt")]
    public int MinWorkerThreads { get; set; } = HardwareInfo.ProcessorCount;

    [CommandLineArgument]
    [Description("Min. thread pool IO thread count.")]
    [ValueDescription("Number")]
    [ValidateRange(1, null)]
    [Alias("iot")]
    public int MinIOThreads { get; set; } = HardwareInfo.ProcessorCount * 10;

    [CommandLineArgument]
    [Description("ByteSerializer to use in Stl.Rpc tests.")]
    [ValueDescription("MessagePack,MemoryPack")]
    [ValidateRange(0, null)]
    [Alias("bs")]
    public ByteSerializerKind ByteSerializer { get; set; } = ByteSerializerKind.MessagePack;
}
