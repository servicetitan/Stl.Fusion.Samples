using System.Runtime.Serialization;
using MemoryPack;

namespace Samples.RpcBenchmark;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial class Item
{
    [DataMember, MemoryPackOrder(0)] public long Index { get; set; }
    [DataMember, MemoryPackOrder(1)] public byte[]? Data { get; set; }
}
