using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using Samples.Blazor.Abstractions;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController, JsonifyErrors]
    public class ChatController : ControllerBase, IChatService
    {
        private readonly IChatService _chat;
        private readonly ISessionResolver _sessionResolver;

        public ChatController(IChatService chat, ISessionResolver sessionResolver)
        {
            _chat = chat;
            _sessionResolver = sessionResolver;
        }

        // Commands

        [HttpPost("postMessage")]
        public Task<ChatMessage> PostMessageAsync([FromBody] IChatService.PostMessageCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(_sessionResolver);
            return _chat.PostMessageAsync(command, cancellationToken);
        }

        // Queries

        [HttpGet("getCurrentUser"), Publish]
        public Task<ChatUser> GetCurrentUserAsync(Session? session, CancellationToken cancellationToken = default)
        {
            session ??= _sessionResolver.Session;
            return _chat.GetCurrentUserAsync(session, cancellationToken);
        }

        [HttpGet("getUser"), Publish]
        public Task<ChatUser> GetUserAsync(long id, CancellationToken cancellationToken = default)
            => _chat.GetUserAsync(id, cancellationToken);

        [HttpGet("getUserCount"), Publish]
        public Task<long> GetUserCountAsync(CancellationToken cancellationToken = default)
            => _chat.GetUserCountAsync(cancellationToken);

        [HttpGet("getActiveUserCount"), Publish]
        public Task<long> GetActiveUserCountAsync(CancellationToken cancellationToken = default)
            => _chat.GetActiveUserCountAsync(cancellationToken);

        [HttpGet("getChatTail"), Publish]
        public Task<ChatPage> GetChatTailAsync(int length, CancellationToken cancellationToken = default)
            => _chat.GetChatTailAsync(length, cancellationToken);

        [HttpGet("getChatPage"), Publish]
        public Task<ChatPage> GetChatPageAsync(long minMessageId, long maxMessageId, CancellationToken cancellationToken = default)
            => _chat.GetChatPageAsync(minMessageId, maxMessageId, cancellationToken);
    }
}
