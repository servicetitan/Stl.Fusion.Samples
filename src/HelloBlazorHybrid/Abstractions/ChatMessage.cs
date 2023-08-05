using System.Runtime.Serialization;
using MemoryPack;

namespace Samples.HelloBlazorHybrid.Abstractions;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public partial record ChatMessage(
    [property: DataMember, MemoryPackOrder(0)] DateTime Time,
    [property: DataMember, MemoryPackOrder(1)] string Name,
    [property: DataMember, MemoryPackOrder(2)] string Text
);
