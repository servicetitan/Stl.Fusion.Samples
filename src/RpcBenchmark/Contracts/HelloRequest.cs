using System.Runtime.Serialization;
using MemoryPack;

namespace Samples.RpcBenchmark;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial class HelloRequest
{
    [DataMember, MemoryPackOrder(0)] public Hello Request { get; set; } = null!;
}
