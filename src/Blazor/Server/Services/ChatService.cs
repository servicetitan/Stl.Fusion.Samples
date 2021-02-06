using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stl.Async;
using Stl.Fusion;
using Stl.Fusion.Bridge;
using Samples.Blazor.Abstractions;
using Stl.CommandR;
using Stl.CommandR.Configuration;
using Stl.Fusion.Authentication;
using Stl.Fusion.Authentication.Commands;
using Stl.Fusion.EntityFramework;
using Stl.Fusion.Operations;

namespace Samples.Blazor.Server.Services
{
    [ComputeService(typeof(IChatService))]
    public class ChatService : DbServiceBase<AppDbContext>, IChatService
    {
        private readonly ILogger _log;
        private readonly IServerSideAuthService _authService;
        private readonly IForismaticClient _forismaticClient;

        public ChatService(
            IServerSideAuthService authService,
            IForismaticClient forismaticClient,
            IServiceProvider services,
            ILogger<ChatService>? log = null)
            : base(services)
        {
            _log = log ??= NullLogger<ChatService>.Instance;
            _authService = authService;
            _forismaticClient = forismaticClient;
        }


        // Commands

        public virtual async Task<ChatMessage> PostMessageAsync(
            IChatService.PostMessageCommand command, CancellationToken cancellationToken = default)
        {
            var (text, session) = command;
            var context = CommandContext.GetCurrent();
            if (Computed.IsInvalidating()) {
                PseudoGetAnyChatTailAsync().Ignore();
                return default!;
            }

            text = await NormalizeTextAsync(text, cancellationToken);
            var user = await GetCurrentUserAsync(session, cancellationToken);
            if (user == null)
                throw new InvalidOperationException("No current ChatUser. Call SetUserNameAsync to create it.");

            await using var dbContext = await CreateCommandDbContextAsync(cancellationToken);
            var message = new ChatMessage() {
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id,
                Text = text,
            };
            await dbContext.ChatMessages.AddAsync(message, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return message;
        }

        // Queries

        [ComputeMethod(KeepAliveTime = 61, AutoInvalidateTime = 60)]
        public virtual async Task<long> GetUserCountAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            return await dbContext.Users.AsQueryable().LongCountAsync(cancellationToken);
        }

        [ComputeMethod(KeepAliveTime = 61, AutoInvalidateTime = 60)]
        public virtual async Task<long> GetActiveUserCountAsync(CancellationToken cancellationToken = default)
        {
            var minLastSeenAt = (Clock.Now - TimeSpan.FromMinutes(5)).ToDateTime();
            await using var dbContext = CreateDbContext();
            return await dbContext.Sessions.AsQueryable()
                .Where(s => s.LastSeenAt >= minLastSeenAt)
                .Select(s => s.UserId)
                .Distinct()
                .LongCountAsync(cancellationToken);
        }

        public virtual async Task<ChatUser> GetCurrentUserAsync(Session session, CancellationToken cancellationToken = default)
        {
            var user = await _authService.GetUserAsync(session, cancellationToken);
            return ToChatUser(user);
        }

        public virtual async Task<ChatUser> GetUserAsync(long id, CancellationToken cancellationToken = default)
        {
            var user = await _authService.TryGetUserAsync(id.ToString(), cancellationToken);
            return ToChatUser(user ?? throw new KeyNotFoundException());
        }

        public virtual async Task<ChatPage> GetChatTailAsync(int length, CancellationToken cancellationToken = default)
        {
            await PseudoGetAnyChatTailAsync();
            await using var dbContext = CreateDbContext();

            // Fetching messages from DB
            var messages = await dbContext.ChatMessages.AsQueryable()
                .OrderByDescending(m => m.Id)
                .Take(length)
                .ToListAsync(cancellationToken);
            messages.Reverse();

            // Fetching users via GetUserAsync
            var userIds = messages.Select(m => m.UserId).Distinct().ToArray();
            var userTasks = userIds.Select(id => GetUserAsync(id, cancellationToken));
            var users = await Task.WhenAll(userTasks);

            // Composing the end result
            return new ChatPage(messages, users.ToDictionary(u => u.Id));
        }

        public virtual Task<ChatPage> GetChatPageAsync(long minMessageId, long maxMessageId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        // Helpers

        [ComputeMethod]
        protected virtual Task<Unit> PseudoGetAnyChatTailAsync() => TaskEx.UnitTask;

        [CommandHandler(IsFilter = true, Priority = 1)]
        protected virtual async Task OnSignInAsync(SignInCommand command, CancellationToken cancellationToken)
        {
            var context = CommandContext.GetCurrent();
            await context.InvokeRemainingHandlersAsync(cancellationToken);
            if (Computed.IsInvalidating()) {
                var isNewUser = context.Operation().Items.TryGet<bool>();
                if (isNewUser) {
                    GetUserCountAsync(default).Ignore();
                    GetActiveUserCountAsync(default).Ignore();
                }
            }
        }

        private async ValueTask<string> NormalizeTextAsync(
            string text, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(text))
                return text;
            var json = await _forismaticClient.GetQuoteAsync(cancellationToken: cancellationToken);
            return json.Value<string>("quoteText");
        }

        private ChatUser ToChatUser(User? user)
        {
            if (user == null || !long.TryParse(user.Id, out var userId))
                return ChatUser.None;
            return new() {
                Id = userId,
                Name = user.Name,
            };
        }
    }
}
