using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Samples.HelloBlazorHybrid.Abstractions;
using Stl.Fusion.Server;

namespace Samples.HelloBlazorHybrid.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController, JsonifyErrors]
public class ChatController : ControllerBase, IChatService
{
    private readonly IChatService _chat;

    public ChatController(IChatService chat) => _chat = chat;

    [HttpGet, Publish]
    public Task<int> GetMessageCount()
        => _chat.GetMessageCount();

    [HttpGet, Publish]
    public Task<Unit> GetAnyTail()
        => _chat.GetAnyTail();

    [HttpGet, Publish]
    public Task<IChatService.Message[]> GetMessages(int count, CancellationToken cancellationToken = default)
        => _chat.GetMessages(count, cancellationToken);

    [HttpPost]
    public Task PostMessage([FromBody] IChatService.PostCommand command,
        CancellationToken cancellationToken = default)
        => _chat.PostMessage(command, cancellationToken);
}
