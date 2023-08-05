using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using MemoryPack;
using Microsoft.EntityFrameworkCore;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.Abstractions;

// Entity
[Index(nameof(UserId))]
[Index(nameof(CreatedAt))]
[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public partial class ChatMessage
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [DataMember, MemoryPackOrder(0)] public long Id { get; set; }
    [DataMember, MemoryPackOrder(1)] public string UserId { get; set; } = "";
    [DataMember, MemoryPackOrder(2)] public DateTime CreatedAt { get; set; }
    [Required, MaxLength(4000)]
    [DataMember, MemoryPackOrder(3)] public string Text { get; set; } = "";
}

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public partial record ChatMessageList
{
    // Must be sorted by ChatMessage.Id
    [DataMember, MemoryPackOrder(0)] public ImmutableArray<ChatMessage> Messages { get; init; } = ImmutableArray<ChatMessage>.Empty;
    [DataMember, MemoryPackOrder(1)] public ImmutableDictionary<string, User> Users { get; init; } = ImmutableDictionary<string, User>.Empty;
}

public interface IChatService : IComputeService
{
    // Commands
    [CommandHandler]
    Task<ChatMessage> Post(Chat_Post command, CancellationToken cancellationToken = default);

    // Queries
    [ComputeMethod]
    Task<ChatMessageList> GetChatTail(int length, CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<long> GetUserCount(CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<long> GetActiveUserCount(CancellationToken cancellationToken = default);
}

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
// ReSharper disable once InconsistentNaming
public partial record Chat_Post(
    [property: DataMember, MemoryPackOrder(0)] string Text,
    [property: DataMember, MemoryPackOrder(1)] Session Session
    ) : ISessionCommand<ChatMessage>;
