using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using MemoryPack;

namespace Samples.Benchmark;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record TestItem : IHasId<long>
{
    [Key]
    [DataMember, MemoryPackOrder(0)] public long Id { get; init; }
    [DataMember, MemoryPackOrder(1)] public long Version { get; init; }
    [DataMember, MemoryPackOrder(2)] public DateTime CreatedAt { get; init; }
    [DataMember, MemoryPackOrder(3)] public DateTime ModifiedAt { get; init; }
    [DataMember, MemoryPackOrder(4)] public string Name { get; init; } = "";
}
