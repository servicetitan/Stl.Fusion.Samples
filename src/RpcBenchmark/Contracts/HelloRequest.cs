using System.Runtime.Serialization;
using MemoryPack;

namespace Samples.RpcBenchmark;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial class HelloRequest
{
    public static HelloRequest ExamplePayload = new() { Request = Hello.ExamplePayload };

    [DataMember, MemoryPackOrder(0)] public Hello Request { get; set; } = null!;
}
