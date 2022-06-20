using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Samples.Blazor.Abstractions;

public class LongKeyedEntity : IHasId<long>
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
}

[Index(nameof(UserId))]
[Index(nameof(CreatedAt))]
public class ChatMessage : LongKeyedEntity
{
    public long UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    [Required, MaxLength(4000)]
    public string Text { get; set; } = "";
}

public class ChatPage
{
    [JsonIgnore]
    public long? MinMessageId { get; }
    [JsonIgnore]
    public long? MaxMessageId { get; }
    // Must be sorted by ChatMessage.Id
    public List<ChatMessage> Messages { get; }
    public Dictionary<long, ChatUser> Users { get; }

    public ChatPage()
        : this(new List<ChatMessage>(), new Dictionary<long, ChatUser>()) { }
    [JsonConstructor]
    public ChatPage(List<ChatMessage> messages, Dictionary<long, ChatUser> users)
    {
        Messages = messages;
        Users = users;
        if (messages.Count > 0) {
            MinMessageId = messages.Min(m => m.Id);
            MaxMessageId = messages.Max(m => m.Id);
        }
    }
}

// Not an entity!
public class ChatUser : LongKeyedEntity
{
    public static ChatUser None => new() { Id = -1, Name = "No user found" };

    [Required, MaxLength(120)]
    public string Name { get; set; } = "";
    public bool IsValid => Id >= 0;
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
    [ComputeMethod(KeepAliveTime = 11)]
    Task<ChatUser> GetCurrentUser(Session session, CancellationToken cancellationToken = default);
    [ComputeMethod(KeepAliveTime = 1)]
    Task<ChatUser> GetUser(long id, CancellationToken cancellationToken = default);
    [ComputeMethod(KeepAliveTime = 61)]
    Task<long> GetUserCount(CancellationToken cancellationToken = default);
    [ComputeMethod(KeepAliveTime = 61)]
    Task<long> GetActiveUserCount(CancellationToken cancellationToken = default);
    [ComputeMethod(KeepAliveTime = 11)]
    Task<ChatPage> GetChatTail(int length, CancellationToken cancellationToken = default);
    [ComputeMethod(KeepAliveTime = 1)]
    Task<ChatPage> GetChatPage(long minMessageId, long maxMessageId, CancellationToken cancellationToken = default);
}
