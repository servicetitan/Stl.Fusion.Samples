namespace Samples.HelloBlazorHybrid.Abstractions;

public interface IChatService
{
    public record PostCommand(string Name, string Text) : ICommand<Unit>;

    public record Message(DateTime Time, string Name, string Text);

    [CommandHandler]
    Task PostMessage(PostCommand command, CancellationToken cancellationToken = default);

    [ComputeMethod]
    Task<int> GetMessageCount();

    [ComputeMethod]
    Task<Message[]> GetMessages(int count, CancellationToken cancellationToken = default);

    [ComputeMethod]
    Task<Unit> GetAnyTail();
}
