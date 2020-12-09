using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController, JsonifyErrors]
    public class ChatController : ControllerBase, IChatService
    {
        private readonly IChatService _chat;

        public ChatController(IChatService chat) => _chat = chat;

        // Writers

        [HttpPost("createUser")]
        public Task<ChatUser> CreateUserAsync(string? name, CancellationToken cancellationToken = default)
        {
            name ??= "";
            return _chat.CreateUserAsync(name, cancellationToken);
        }

        [HttpPost("setUserName")]
        public async Task<ChatUser> SetUserNameAsync(long id, string? name, CancellationToken cancellationToken = default)
        {
            name ??= "";
            return await _chat.SetUserNameAsync(id, name, cancellationToken);
        }

        [HttpPost("addMessage")]
        public async Task<ChatMessage> AddMessageAsync(long userId, string? text, CancellationToken cancellationToken = default)
        {
            text ??= "";
            return await _chat.AddMessageAsync(userId, text, cancellationToken);
        }

        // Readers

        [HttpGet("getUserCount"), Publish]
        public Task<long> GetUserCountAsync(CancellationToken cancellationToken = default)
            => _chat.GetUserCountAsync(cancellationToken);

        [HttpGet("getActiveUserCount"), Publish]
        public Task<long> GetActiveUserCountAsync(CancellationToken cancellationToken = default)
            => _chat.GetActiveUserCountAsync(cancellationToken);

        [HttpGet("getUser"), Publish]
        public Task<ChatUser> GetUserAsync(long id, CancellationToken cancellationToken = default)
            => _chat.GetUserAsync(id, cancellationToken);

        [HttpGet("getChatTail"), Publish]
        public Task<ChatPage> GetChatTailAsync(int length, CancellationToken cancellationToken = default)
            => _chat.GetChatTailAsync(length, cancellationToken);

        [HttpGet("getChatPage"), Publish]
        public Task<ChatPage> GetChatPageAsync(long minMessageId, long maxMessageId, CancellationToken cancellationToken = default)
            => _chat.GetChatPageAsync(minMessageId, maxMessageId, cancellationToken);
    }
}
