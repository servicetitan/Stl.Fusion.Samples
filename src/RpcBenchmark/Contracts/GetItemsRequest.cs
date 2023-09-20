using System.Runtime.Serialization;
using MemoryPack;

namespace Samples.RpcBenchmark;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial class GetItemsRequest
{
    [DataMember, MemoryPackOrder(0)] public int DataSize { get; set; }
    [DataMember, MemoryPackOrder(1)] public int DelayEvery { get; set; } = 1;
    [DataMember, MemoryPackOrder(2)] public int Count { get; set; }
}
