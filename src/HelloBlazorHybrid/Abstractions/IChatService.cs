namespace Samples.HelloBlazorHybrid.Abstractions;

public interface IChatService
{
    public record PostCommand(string Name, string Text) : ICommand<Unit>
    {
        // Default constructor is needed for JSON deserialization
        public PostCommand() : this(null!, null!) { }
    }

    public record Message(DateTime Time, string Name, string Text)
    {
        // Default constructor is needed for JSON deserialization
        public Message() : this(default, null!, null!) { }
    }

    [CommandHandler]
    Task PostMessage(PostCommand command, CancellationToken cancellationToken = default);

    [ComputeMethod]
    Task<int> GetMessageCount();

    [ComputeMethod]
    Task<Message[]> GetMessages(int count, CancellationToken cancellationToken = default);

    [ComputeMethod]
    Task<Unit> GetAnyTail();
}
