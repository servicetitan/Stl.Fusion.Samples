using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using Samples.Blazor.Abstractions;

namespace Samples.Blazor.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController, JsonifyErrors, UseDefaultSession]
public class ChatController : ControllerBase, IChatService
{
    private readonly IChatService _chat;

    public ChatController(IChatService chat) 
        => _chat = chat;

    // Commands

    [HttpPost]
    public Task<ChatMessage> Post([FromBody] IChatService.PostCommand command, CancellationToken cancellationToken = default)
        => _chat.Post(command, cancellationToken);

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
