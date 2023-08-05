using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Samples.Blazor.Abstractions;
using Stl.Fusion.Authentication;
using Stl.Fusion.EntityFramework;

namespace Samples.Blazor.Server.Services;

public class ChatService : DbServiceBase<AppDbContext>, IChatService
{
    private readonly ILogger _log;
    private readonly IAuth _auth;
    private readonly IAuthBackend _authBackend;
    private readonly IForismaticClient _forismaticClient;

    public ChatService(
        IAuth auth,
        IAuthBackend authBackend,
        IForismaticClient forismaticClient,
        IServiceProvider services,
        ILogger<ChatService>? log = null)
        : base(services)
    {
        _log = log ?? NullLogger<ChatService>.Instance;
        _auth = auth;
        _authBackend = authBackend;
        _forismaticClient = forismaticClient;
    }

    // Commands

    public virtual async Task<ChatMessage> Post(
        Chat_Post command, CancellationToken cancellationToken = default)
    {
        var (text, session) = command;
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating()) {
            _ = PseudoGetAnyChatTail();
            return default!;
        }

        text = await NormalizeText(text, cancellationToken);
        var user = await _auth.GetUser(session, cancellationToken).Require();

        await using var dbContext = await CreateCommandDbContext(cancellationToken);
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

    [ComputeMethod(AutoInvalidationDelay = 60)]
    public virtual async Task<long> GetUserCount(CancellationToken cancellationToken = default)
    {
        await using var dbContext = CreateDbContext();
        return await dbContext.Users.AsQueryable().LongCountAsync(cancellationToken);
    }

    [ComputeMethod(AutoInvalidationDelay = 60)]
    public virtual async Task<long> GetActiveUserCount(CancellationToken cancellationToken = default)
    {
        var minLastSeenAt = (Clocks.SystemClock.Now - TimeSpan.FromMinutes(5)).ToDateTime();
        await using var dbContext = CreateDbContext();
        return await dbContext.Sessions.AsQueryable()
            .Where(s => s.LastSeenAt >= minLastSeenAt)
            .Select(s => s.UserId)
            .Distinct()
            .LongCountAsync(cancellationToken);
    }

    public virtual async Task<ChatMessageList> GetChatTail(int length, CancellationToken cancellationToken = default)
    {
        await PseudoGetAnyChatTail();
        await using var dbContext = CreateDbContext();

        // Fetching messages from DB
        var messages = await dbContext.ChatMessages.AsQueryable()
            .OrderByDescending(m => m.Id)
            .Take(length)
            .ToListAsync(cancellationToken);
        messages.Reverse();

        // Fetching users via GetUserAsync
        var userIds = messages.Select(m => m.UserId).Distinct().ToArray();
        var userTasks = userIds.Select(async id => {
            var user = await _authBackend.GetUser(default, id, cancellationToken);
            return user.OrGuest("<Deleted user>").ToClientSideUser();
        });
        var users = (await Task.WhenAll(userTasks)).OfType<User>();

        // Composing the end result
        return new ChatMessageList() {
            Messages = messages.ToImmutableArray(),
            Users = users.ToImmutableDictionary(u => u.Id.Value),
        };
    }

    // Helpers

    [ComputeMethod]
    protected virtual Task<Unit> PseudoGetAnyChatTail() => TaskExt.UnitTask;

    [CommandHandler(IsFilter = true, Priority = 1)]
    protected virtual async Task OnSignIn(AuthBackend_SignIn command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        await context.InvokeRemainingHandlers(cancellationToken);
        if (Computed.IsInvalidating()) {
            var isNewUser = context.Operation().Items.GetOrDefault(false);
            if (isNewUser) {
                _ = GetUserCount(default);
                _ = GetActiveUserCount(default);
            }
        }
    }

    private async ValueTask<string> NormalizeText(
        string text, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(text))
            return text;
        var json = await _forismaticClient.GetQuote(cancellationToken: cancellationToken);
        var jObject = JObject.Parse(json);
        return jObject.Value<string>("quoteText")!;
    }
}
