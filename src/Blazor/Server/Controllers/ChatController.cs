using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using Samples.Blazor.Abstractions;

namespace Samples.Blazor.Server.Controllers;

[Route("api/[controller]/[action]")]
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

    [HttpPost]
    public Task<ChatMessage> Post([FromBody] IChatService.PostCommand command, CancellationToken cancellationToken = default)
        => _chat.Post(command.UseDefaultSession(_sessionResolver), cancellationToken);

    // Queries

    [HttpGet, Publish]
    public Task<ChatUser> GetCurrentUser(Session session, CancellationToken cancellationToken = default)
        => _chat.GetCurrentUser(session, cancellationToken);

    [HttpGet, Publish]
    public Task<ChatUser> GetUser(long id, CancellationToken cancellationToken = default)
        => _chat.GetUser(id, cancellationToken);

    [HttpGet, Publish]
    public Task<long> GetUserCount(CancellationToken cancellationToken = default)
        => _chat.GetUserCount(cancellationToken);

    [HttpGet, Publish]
    public Task<long> GetActiveUserCount(CancellationToken cancellationToken = default)
        => _chat.GetActiveUserCount(cancellationToken);

    [HttpGet, Publish]
    public Task<ChatPage> GetChatTail(int length, CancellationToken cancellationToken = default)
        => _chat.GetChatTail(length, cancellationToken);

    [HttpGet, Publish]
    public Task<ChatPage> GetChatPage(long minMessageId, long maxMessageId, CancellationToken cancellationToken = default)
        => _chat.GetChatPage(minMessageId, maxMessageId, cancellationToken);
}
