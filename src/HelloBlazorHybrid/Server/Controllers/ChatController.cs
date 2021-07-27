using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Samples.HelloBlazorHybrid.Abstractions;
using Stl.Fusion.Server;

namespace Samples.HelloBlazorHybrid.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController, JsonifyErrors]
    public class ChatController : ControllerBase, IChatService
    {
        private readonly IChatService _chat;

        public ChatController(IChatService chat) => _chat = chat;

        [HttpGet, Publish]
        public Task<int> GetMessageCountAsync()
            => _chat.GetMessageCountAsync();

        [HttpGet, Publish]
        public Task<Unit> GetAnyTailAsync()
            => _chat.GetAnyTailAsync();

        [HttpGet, Publish]
        public Task<IChatService.Message[]> GetMessagesAsync(int count, CancellationToken cancellationToken = default)
            => _chat.GetMessagesAsync(count, cancellationToken);

        [HttpPost]
        public Task PostMessageAsync([FromBody] IChatService.PostCommand command,
            CancellationToken cancellationToken = default)
            => _chat.PostMessageAsync(command, cancellationToken);
    }
}
