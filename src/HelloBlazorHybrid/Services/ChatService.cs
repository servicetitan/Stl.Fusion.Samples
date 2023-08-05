using Samples.HelloBlazorHybrid.Abstractions;

namespace Samples.HelloBlazorHybrid.Services;

public class ChatService : IChatService
{
    private volatile ImmutableList<ChatMessage> _messages = ImmutableList<ChatMessage>.Empty;

    private readonly object _lock = new();

    [CommandHandler]
    public virtual Task PostMessage(Chat_Post command, CancellationToken cancellationToken = default)
    {
        if (Computed.IsInvalidating()) {
            _ = GetMessageCount();
            _ = GetAnyTail();
            return Task.CompletedTask;
        }

        var (name, message) = command;
        lock (_lock) {
            _messages = _messages.Add(new(DateTime.Now, name, message));
        }
        return Task.CompletedTask;
    }

    [ComputeMethod]
    public virtual Task<int> GetMessageCount()
        => Task.FromResult(_messages.Count);

    [ComputeMethod]
    public virtual async Task<ChatMessage[]> GetMessages(
        int count, CancellationToken cancellationToken = default)
    {
        // Fake dependency used to invalidate all GetMessages(...) independently on count argument
        await GetAnyTail();
        return _messages.TakeLast(count).ToArray();
    }

    [ComputeMethod]
    public virtual Task<Unit> GetAnyTail() => TaskExt.UnitTask;
}
