using System.Runtime.Serialization;
using MemoryPack;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.Abstractions;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public partial record ComposedValue
{
    [DataMember, MemoryPackOrder(0)] public string Parameter { get; init; } = "";
    [DataMember, MemoryPackOrder(1)] public double Uptime { get; init; }
    [DataMember, MemoryPackOrder(2)] public double? Sum { get; init; }
    [DataMember, MemoryPackOrder(3)] public string LastChatMessage { get; init; } = "";
    [DataMember, MemoryPackOrder(4)] public User? User { get; init; }
    [DataMember, MemoryPackOrder(5)] public long ActiveUserCount { get; init; }
}

public interface IComposerService : IComputeService
{
    [ComputeMethod]
    Task<ComposedValue> GetComposedValue(
        Session session, string parameter,
        CancellationToken cancellationToken = default);
}

public interface ILocalComposerService : IComposerService { }
