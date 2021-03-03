using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stl.CommandR;
using Stl.CommandR.Configuration;
using static System.Console;

var services = new ServiceCollection()
    .AddCommander(c => c.AddCommandService<GreetingService>())
    .BuildServiceProvider();
var greetingService = services.GetRequiredService<GreetingService>();
var commander = services.Commander();

await greetingService.OnSayCommandAsync(new SayCommand("Hello!"));
await greetingService.OnSayCommandOverrideAsync(new SayCommand("There!"));
await commander.CallAsync(new SayCommand("All these calls work the same way!"));
await commander.RunAsync(new SayCommand("")); // This call won't throw an exception

// Types used in this example

public record SayCommand(string Text) : ICommand<Unit>
{
    public SayCommand() : this("") { }
}

public class GreetingService
{
    [CommandHandler]
    public virtual Task OnSayCommandAsync(SayCommand command, CancellationToken cancellationToken = default)
    {
        WriteLine(command.Text);
        return Task.CompletedTask;
    }

    [CommandHandler(Priority = 1, IsFilter = true)] // Fake filter: >1 non-filtering handlers per command = error
    public virtual async Task OnSayCommandOverrideAsync(SayCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(command.Text))
            throw new ArgumentOutOfRangeException(nameof(command));
        var context = CommandContext.GetCurrent();
        WriteLine($"{command.Text} (override)");
        if (command.Text.Contains("T"))
            await context.InvokeRemainingHandlersAsync(cancellationToken);
    }

    [CommandHandler(Priority = 10, IsFilter = true)]
    public virtual async Task OnAnyCommandAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        WriteLine($"> {command}");
        try {
            await context.InvokeRemainingHandlersAsync(cancellationToken);
        }
        catch (Exception e) {
            WriteLine($"! {command} -> Error: {e.Message}");
        }
        finally {
            WriteLine($"< {command} -> {context.UntypedResult.Value}");
        }
    }
}
