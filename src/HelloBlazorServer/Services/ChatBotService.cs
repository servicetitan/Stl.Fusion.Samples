using Microsoft.Extensions.Hosting;

namespace Samples.HelloBlazorServer.Services;

public class ChatBotService : IComputeService, IHostedService
{
    private static string Morpheus = "M0rpheus";
    private static string MorpheusMessage1 =
        "This is your last chance. After this, there is no turning back. " +
        "You take the blue pill—the story ends, you wake up in your bed and believe whatever you want to believe. " +
        "You take the red pill—you stay in Wonderland and I show you how deep the rabbit-hole goes.";
    private static string MorpheusMessage2 =
        "I'm trying to free your mind, Neo. But I can only show you the door. " +
        "You're the one that has to walk through it.";
    private static string Groot = "Groot";
    private static string GrootMessage = "I am Groot!";
    private static string TimeBot = "Time Bot";
    private static readonly HashSet<string> BotNames = new() {Morpheus, Groot, TimeBot};

    private readonly ChatService _chatService;
    private readonly ICommander _commander;

    public ChatBotService(ChatService chatService, ICommander commander)
    {
        _chatService = chatService;
        _commander = commander;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
        => await _commander.Call(new Chat_Post(Morpheus, MorpheusMessage1), cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [CommandHandler(Priority = 1, IsFilter = true)]
    protected virtual async Task OnChatPost(Chat_Post command, CancellationToken cancellationToken)
    {
        await CommandContext.GetCurrent().InvokeRemainingHandlers(cancellationToken);
        if (Computed.IsInvalidating()) {
            // We know for sure here the command has completed successfully
            // Now we need to suppress ExecutionContext flow to ensure
            // Reaction runs its commands outside of the current command context,
            // outside Computed.Invalidate() block, etc.
            using var suppressing = ExecutionContextExt.TrySuppressFlow();
            _ = Task.Run(() => Reaction(command, default), default);
        }
    }

    protected virtual async Task Reaction(Chat_Post command, CancellationToken cancellationToken)
    {
        var messageCount = await _chatService.GetMessageCount();
        switch (messageCount) {
        case 1:
            break;
        case 2:
            await Task.Delay(1000);
            await _commander.Call(new Chat_Post(Morpheus, MorpheusMessage2), default);
            break;
        default:
            var messages = await _chatService.GetMessages(1, cancellationToken);
            var (time, name, message) = messages.SingleOrDefault();
            name ??= "";
            if (name == "" || BotNames.Contains(name))
                break;
            if (message.ToLowerInvariant().Contains("time"))
                await _commander.Call(new Chat_Post(TimeBot, DateTime.Now.ToString("F")), default);
            else
                await _commander.Call(new Chat_Post(Groot, GrootMessage), default);
            break;
        }
    }
}
