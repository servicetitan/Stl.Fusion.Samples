namespace Samples.HelloBlazorServer.Services;

public class ChatService : IComputeService
{
    private volatile ImmutableList<(DateTime Time, string Name, string Message)> _messages =
        ImmutableList<(DateTime, string, string)>.Empty;
    private readonly object _lock = new();

    [CommandHandler]
    public virtual Task PostMessage(Chat_Post command, CancellationToken cancellationToken = default)
    {
        if (Computed.IsInvalidating()) {
            _ = GetMessageCount();
            _ = PseudoGetAnyTail();
            return Task.CompletedTask;
        }

        var (name, message) = command;
        lock (_lock) {
            _messages = _messages.Add((DateTime.Now, name, message));
        }
        return Task.CompletedTask;
    }

    [ComputeMethod]
    public virtual Task<int> GetMessageCount()
        => Task.FromResult(_messages.Count);

    [ComputeMethod]
    public virtual async Task<(DateTime Time, string Name, string Message)[]> GetMessages(
        int count, CancellationToken cancellationToken = default)
    {
        // Fake dependency used to invalidate all GetMessages(...) independently on count argument
        await PseudoGetAnyTail();
        return _messages.TakeLast(count).ToArray();
    }

    [ComputeMethod]
    protected virtual Task<Unit> PseudoGetAnyTail() => TaskExt.UnitTask;
}

// ReSharper disable once InconsistentNaming
public record Chat_Post(string Name, string Message) : ICommand<Unit>;
