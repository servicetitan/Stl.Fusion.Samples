using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stl;
using Stl.CommandR.Configuration;
using Stl.Fusion;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.Abstractions
{
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
        public record PostMessageCommand(string Text, Session Session) : ISessionCommand<ChatMessage>
        {
            // Default constructor is needed for JSON deserialization
            public PostMessageCommand() : this(null!, Session.Null) { }
        }

        // Commands
        [CommandHandler]
        Task<ChatMessage> PostMessageAsync(PostMessageCommand command, CancellationToken cancellationToken = default);

        // Queries
        [ComputeMethod(KeepAliveTime = 11)]
        Task<ChatUser> GetCurrentUserAsync(Session session, CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 1)]
        Task<ChatUser> GetUserAsync(long id, CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 61)]
        Task<long> GetUserCountAsync(CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 61)]
        Task<long> GetActiveUserCountAsync(CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 11)]
        Task<ChatPage> GetChatTailAsync(int length, CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 1)]
        Task<ChatPage> GetChatPageAsync(long minMessageId, long maxMessageId, CancellationToken cancellationToken = default);
    }
}
