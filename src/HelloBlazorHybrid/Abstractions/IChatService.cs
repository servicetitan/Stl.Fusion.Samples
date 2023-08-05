using System.Runtime.Serialization;
using MemoryPack;

namespace Samples.HelloBlazorHybrid.Abstractions;

public interface IChatService : IComputeService
{
    [CommandHandler]
    Task PostMessage(Chat_Post command, CancellationToken cancellationToken = default);

    [ComputeMethod]
    Task<int> GetMessageCount();
    [ComputeMethod]
    Task<ChatMessage[]> GetMessages(int count, CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<Unit> GetAnyTail();
}

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
// ReSharper disable once InconsistentNaming
public partial record Chat_Post(
    [property: DataMember, MemoryPackOrder(0)] string Name,
    [property: DataMember, MemoryPackOrder(1)] string Text
    ) : ICommand<Unit>;
