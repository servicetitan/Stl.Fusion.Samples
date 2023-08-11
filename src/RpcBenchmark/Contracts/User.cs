using System.Runtime.Serialization;
using MemoryPack;

namespace Samples.RpcBenchmark;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial class User : IHasId<long>
{
    [DataMember, MemoryPackOrder(0)] public long Id { get; set; }
    [DataMember, MemoryPackOrder(1)] public long Version { get; set; }
    [DataMember, MemoryPackOrder(2)] public DateTime CreatedAt { get; set; }
    [DataMember, MemoryPackOrder(3)] public DateTime ModifiedAt { get; set; }
    [DataMember, MemoryPackOrder(4)] public string Name { get; set; } = "";
}
