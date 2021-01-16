using System;
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
using Stl.Fusion.Authentication;
using Stl.Fusion.EntityFramework;
using Stl.Fusion.Operations;

namespace Samples.Blazor.Server.Services
{
    [ComputeService(typeof(IChatService))]
    public class ChatService : DbServiceBase<AppDbContext>, IChatService
    {
        private readonly ILogger _log;
        private readonly IUzbyClient _uzbyClient;
        private readonly IForismaticClient _forismaticClient;
        private readonly IAuthService _authService;
        private readonly DbEntityResolver<AppDbContext, long, ChatUser> _userResolver;
        private readonly DbEntityResolver<AppDbContext, long, ChatMessage> _messageResolver;
        private readonly IPublisher _publisher;

        public ChatService(
            IUzbyClient uzbyClient,
            IForismaticClient forismaticClient,
            IAuthService authService,
            DbEntityResolver<AppDbContext, long, ChatUser> userResolver,
            DbEntityResolver<AppDbContext, long, ChatMessage> messageResolver,
            IPublisher publisher,
            IServiceProvider services,
            ILogger<ChatService>? log = null)
            : base(services)
        {
            _log = log ??= NullLogger<ChatService>.Instance;
            _uzbyClient = uzbyClient;
            _forismaticClient = forismaticClient;
            _authService = authService;
            _userResolver = userResolver;
            _messageResolver = messageResolver;
            _publisher = publisher;
        }

        // Commands

        public virtual async Task<ChatUser> SetUserNameAsync(
            IChatService.SetUserNameCommand command, CancellationToken cancellationToken = default)
        {
            var (name, session) = command;
            var context = CommandContext.GetCurrent();
            ChatUser? user;
            if (Computed.IsInvalidating()) {
                user = context.Items.Get<OperationItem<ChatUser>>().Value;
                GetUserAsync(user.Id, cancellationToken).Ignore();
                PseudoGetUserAsync(user.AuthUserId).Ignore();
                if (context.Items.TryGet<OperationItem<bool>>()?.Value ?? false)
                    GetUserCountAsync(cancellationToken).Ignore();
                return default!;
            }

            var authUser = await _authService.GetUserAsync(session, cancellationToken);
            name = await NormalizeNameAsync(name, authUser, cancellationToken);

            await using var dbContext = await CreateCommandDbContextAsync(cancellationToken);
            user = await dbContext.ChatUsers.AsQueryable()
                .SingleOrDefaultAsync(u => u.AuthUserId == authUser.Id, cancellationToken);
            if (user == null) {
                user = new ChatUser() {
                    AuthUserId = authUser.Id,
                    Name = name
                };
                await dbContext.ChatUsers.AddAsync(user, cancellationToken);
                context.Items.Set(OperationItem.New(true)); // Used on invalidation
            }
            else
                user.Name = name;
            await dbContext.SaveChangesAsync(cancellationToken);

            context.Items.Set(OperationItem.New(user)); // Used on invalidation
            return user;
        }

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

        public virtual async Task<long> GetUserCountAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            return await dbContext.ChatUsers.AsQueryable().LongCountAsync(cancellationToken);
        }

        public virtual Task<long> GetActiveUserCountAsync(CancellationToken cancellationToken = default)
        {
            var channelHub = _publisher.ChannelHub;
            var userCount = (long) channelHub.ChannelCount;
            var c = Computed.GetCurrent();
            Task.Run(async () => {
                do {
                    await Task.Delay(1000, default);
                } while (userCount == channelHub.ChannelCount);
                c!.Invalidate();
            }, default).Ignore();
            return Task.FromResult(Math.Max(0, userCount));
        }

        public virtual async Task<ChatUser?> GetCurrentUserAsync(Session session, CancellationToken cancellationToken = default)
        {
            var authUser = await _authService.GetUserAsync(session, cancellationToken);
            await PseudoGetUserAsync(authUser.Id);
            await using var dbContext = CreateDbContext();
            var user = await dbContext.ChatUsers.AsQueryable()
                .SingleOrDefaultAsync(u => u.AuthUserId == authUser.Id, cancellationToken);
            return user?.MaskSecureFields();
        }

        public virtual async Task<ChatUser> GetUserAsync(long id, CancellationToken cancellationToken = default)
        {
            var user = await _userResolver.GetAsync(id, cancellationToken);
            return user.MaskSecureFields();
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
        [ComputeMethod]
        protected virtual Task<Unit> PseudoGetUserAsync(string authUserId) => TaskEx.UnitTask;

        protected virtual async ValueTask<string> NormalizeNameAsync(
            string name, User authUser, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(name))
                return name;
            if (authUser.IsAuthenticated) {
                name = authUser.Name;
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
            name = await _uzbyClient.GetNameAsync(cancellationToken: cancellationToken);
            if (name.GetHashCode() % 3 == 0)
                // First-last name pairs are fun too :)
                name += " " + await _uzbyClient.GetNameAsync(cancellationToken: cancellationToken);
            return name;
        }

        protected virtual async ValueTask<string> NormalizeTextAsync(
            string text, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(text))
                return text;
            var json = await _forismaticClient.GetQuoteAsync(cancellationToken: cancellationToken);
            return json.Value<string>("quoteText");
        }
    }
}
