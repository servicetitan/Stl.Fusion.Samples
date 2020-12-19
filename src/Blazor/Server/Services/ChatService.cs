using System;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stl.Async;
using Stl.Fusion;
using Stl.Fusion.Bridge;
using Samples.Blazor.Abstractions;
using Samples.Helpers;
using Stl;
using Stl.Collections;

namespace Samples.Blazor.Server.Services
{
    [ComputeService(typeof(IChatService))]
    public class ChatService : DbServiceBase<AppDbContext>, IChatService
    {
        private readonly ILogger _log;
        private readonly IUzbyClient _uzbyClient;
        private readonly IForismaticClient _forismaticClient;
        private readonly IPublisher _publisher;

        public ChatService(
            IUzbyClient uzbyClient,
            IForismaticClient forismaticClient,
            IPublisher publisher,
            IServiceProvider services,
            ILogger<ChatService>? log = null)
            : base(services)
        {
            _log = log ??= NullLogger<ChatService>.Instance;
            _uzbyClient = uzbyClient;
            _forismaticClient = forismaticClient;
            _publisher = publisher;
        }

        // Writers

        public async Task<ChatUser> CreateUserAsync(string name, CancellationToken cancellationToken = default)
        {
            name = await NormalizeNameAsync(name, cancellationToken).ConfigureAwait(false);
            await using var dbContext = CreateDbContext();

            var userEntry = dbContext.ChatUsers.Add(new ChatUser() {
                Name = name
            });
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var user = userEntry.Entity;

            // Invalidation
            Computed.Invalidate(() => GetUserAsync(user.Id, CancellationToken.None));
            Computed.Invalidate(() => GetUserCountAsync(CancellationToken.None));
            return user;
        }

        public async Task<ChatUser> SetUserNameAsync(long id, string name, CancellationToken cancellationToken = default)
        {
            name = await NormalizeNameAsync(name, cancellationToken).ConfigureAwait(false);
            await using var dbContext = CreateDbContext();

            var user = await GetUserAsync(id, cancellationToken).ConfigureAwait(false);
            user.Name = name;
            dbContext.ChatUsers.Update(user);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // Invalidation
            Computed.Invalidate(() => GetUserAsync(id, CancellationToken.None));
            return user;
        }

        public async Task<ChatMessage> AddMessageAsync(long userId, string text, CancellationToken cancellationToken = default)
        {
            text = await NormalizeTextAsync(text, cancellationToken).ConfigureAwait(false);
            await using var dbContext = CreateDbContext();

            await GetUserAsync(userId, cancellationToken).ConfigureAwait(false); // Check to ensure the user exists
            var messageEntry = dbContext.ChatMessages.Add(new ChatMessage() {
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                Text = text,
            });
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var message = messageEntry.Entity;

            // Invalidation
            Computed.Invalidate(EveryChatTail);
            return message;
        }

        // Readers

        public virtual async Task<long> GetUserCountAsync(CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();
            return await dbContext.ChatUsers.AsQueryable().LongCountAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual Task<long> GetActiveUserCountAsync(CancellationToken cancellationToken = default)
        {
            var channelHub = _publisher.ChannelHub;
            var userCount = (long) channelHub.ChannelCount;
            var c = Computed.GetCurrent();
            Task.Run(async () => {
                do {
                    await Task.Delay(1000, default).ConfigureAwait(false);
                } while (userCount == channelHub.ChannelCount);
                c!.Invalidate();
            }, default).Ignore();
            return Task.FromResult(Math.Max(0, userCount));
        }

        public virtual Task<ChatUser> GetUserAsync(long id, CancellationToken cancellationToken = default)
        {
            var userResolver = Services.GetRequiredService<ChatUserResolver>();
            return userResolver.GetAsync(id, cancellationToken);
        }

        public virtual async Task<ChatPage> GetChatTailAsync(int length, CancellationToken cancellationToken = default)
        {
            await EveryChatTail().ConfigureAwait(false);
            await using var dbContext = CreateDbContext();

            // Fetching messages from DB
            var messages = await dbContext.ChatMessages.AsQueryable()
                .OrderByDescending(m => m.Id)
                .Take(length)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            messages.Reverse();

            // Fetching users via GetUserAsync
            var userIds = messages.Select(m => m.UserId).Distinct().ToArray();
            var userTasks = userIds.Select(id => GetUserAsync(id, cancellationToken));
            var users = await Task.WhenAll(userTasks).ConfigureAwait(false);

            // Composing the end result
            return new ChatPage(messages, users.ToDictionary(u => u.Id));
        }

        public virtual Task<ChatPage> GetChatPageAsync(long minMessageId, long maxMessageId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        // Helpers

        [ComputeMethod]
        protected virtual Task<Unit> EveryChatTail() => TaskEx.UnitTask;

        protected virtual async ValueTask<string> NormalizeNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(name))
                return name;
            name = await _uzbyClient
                .GetNameAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (name.GetHashCode() % 3 == 0)
                // First-last name pairs are fun too :)
                name += " " + await _uzbyClient
                    .GetNameAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            return name;
        }

        protected virtual async ValueTask<string> NormalizeTextAsync(string text, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(text))
                return text;
            var json = await _forismaticClient
                .GetQuoteAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return json.Value<string>("quoteText");
        }
    }
}
