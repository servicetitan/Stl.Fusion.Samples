using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Samples.Blazor.Abstractions;

// Entity
[Index(nameof(UserId))]
[Index(nameof(CreatedAt))]
public class ChatMessage
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public string UserId { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    [Required, MaxLength(4000)]
    public string Text { get; set; } = "";
}

public record ChatMessageList
{
    // Must be sorted by ChatMessage.Id
    public ImmutableArray<ChatMessage> Messages { get; init; } = ImmutableArray<ChatMessage>.Empty;
    public ImmutableDictionary<string, User> Users { get; init; } = ImmutableDictionary<string, User>.Empty;
}

public interface IChatService
{
    public record PostCommand(string Text, Session Session) : ISessionCommand<ChatMessage>
    {
        // Newtonsoft.Json needs this constructor to deserialize this record
        public PostCommand() : this("", Session.Null) { }
    }

    // Commands
    [CommandHandler]
    Task<ChatMessage> Post(PostCommand command, CancellationToken cancellationToken = default);

    // Queries
    [ComputeMethod]
    Task<ChatMessageList> GetChatTail(int length, CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<long> GetUserCount(CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<long> GetActiveUserCount(CancellationToken cancellationToken = default);
}
