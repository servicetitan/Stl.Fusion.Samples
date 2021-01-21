using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stl.CommandR;
using static System.Console;

namespace Tutorial
{
    public static class Part09
    {
        #region Part09_PrintCommandSession
        public class PrintCommand : ICommand<Unit>
        {
            public string Message { get; set; } = "";
        }

        // Let's start with a classic handler
        public class PrintCommandHandler : ICommandHandler<PrintCommand, Unit>
        {
            public PrintCommandHandler()
            {
                WriteLine("PrintCommandHandler service created.");
            }

            public async Task<Unit> OnCommandAsync(PrintCommand command, CommandContext<Unit> context, CancellationToken cancellationToken)
            {
                WriteLine(command.Message);
                WriteLine("Sir, yes, sir!");
                return default;
            }
        }
        #endregion

        public static async Task PrintCommandSession()
        {
            #region Part09_PrintCommandSession2
            // Building IoC container
            var serviceBuilder = new ServiceCollection()
                .AddScoped<PrintCommandHandler>(); // Try changing this to AddSingleton
            var commanderBuilder = serviceBuilder.AddCommander()
                .AddHandlers<PrintCommandHandler>();
            var services = serviceBuilder.BuildServiceProvider();

            var commander = services.Commander(); // Same as .GetRequiredService<ICommander>()
            await commander.CallAsync(new PrintCommand() { Message = "Are you operational?" });
            await commander.CallAsync(new PrintCommand() { Message = "Are you operational?" });
            #endregion
        }
    }
}
