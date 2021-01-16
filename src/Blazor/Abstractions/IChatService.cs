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

    [Index(nameof(Name), Name = "IX_Name")]
    public class ChatUser : LongKeyedEntity
    {
        [Required, MaxLength(120)]
        public string AuthUserId { get; set; } = "";
        [Required, MaxLength(120)]
        public string Name { get; set; } = "";

        public ChatUser MaskSecureFields()
            => new() {
                Id = Id,
                Name = Name,
            };
    }

    [Index(nameof(UserId), Name = "IX_UserId")]
    [Index(nameof(CreatedAt), Name = "IX_CreatedAt")]
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

    public interface IChatService
    {
        public record SetUserNameCommand(string Name, Session Session) : ISessionCommand<ChatUser> { }
        public record PostMessageCommand(string Text, Session Session) : ISessionCommand<ChatMessage> { }

        // Commands
        [CommandHandler]
        Task<ChatUser> SetUserNameAsync(SetUserNameCommand command, CancellationToken cancellationToken = default);
        [CommandHandler]
        Task<ChatMessage> PostMessageAsync(PostMessageCommand command, CancellationToken cancellationToken = default);

        // Queries
        [ComputeMethod(KeepAliveTime = 10)]
        Task<ChatUser?> GetCurrentUserAsync(Session session, CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 10)]
        Task<long> GetUserCountAsync(CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 10)]
        Task<long> GetActiveUserCountAsync(CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 1)]
        Task<ChatUser> GetUserAsync(long id, CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 10)]
        Task<ChatPage> GetChatTailAsync(int length, CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 1)]
        Task<ChatPage> GetChatPageAsync(long minMessageId, long maxMessageId, CancellationToken cancellationToken = default);
    }
}
