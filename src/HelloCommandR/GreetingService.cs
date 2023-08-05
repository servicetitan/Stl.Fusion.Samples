namespace Samples.HelloCommandR;

public record SayCommand(string Text) : ICommand<Unit>;

public class GreetingService : ICommandService
{
    [CommandHandler]
    public virtual Task OnSayCommand(SayCommand command, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(command.Text);
        return Task.CompletedTask;
    }

    [CommandHandler(Priority = 1, IsFilter = true)]
    public virtual async Task OnSayCommandOverride(SayCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(command.Text))
            throw new ArgumentOutOfRangeException(nameof(command));
        var context = CommandContext.GetCurrent();
        Console.WriteLine($"{command.Text} (override)");
        if (command.Text.Contains("T"))
            await context.InvokeRemainingHandlers(cancellationToken);
    }

    [CommandHandler(Priority = 10, IsFilter = true)]
    public virtual async Task OnAnyCommand(ICommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        Console.WriteLine($"> {command}");
        try {
            await context.InvokeRemainingHandlers(cancellationToken);
        }
        catch (Exception e) {
            Console.WriteLine($"! {command} -> Error: {e.Message}");
        }
        finally {
            Console.WriteLine($"< {command} -> {context.UntypedResult.Value}");
        }
    }
}
