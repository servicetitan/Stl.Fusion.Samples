using System.Runtime.Serialization;
using MemoryPack;

namespace Samples.RpcBenchmark;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial class Pet {
    [DataMember, MemoryPackOrder(0)] public string Name { get; set; } = null!;
    [DataMember, MemoryPackOrder(1)] public Color Color { get; set; }
}
