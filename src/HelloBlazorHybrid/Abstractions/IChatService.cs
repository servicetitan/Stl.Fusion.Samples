using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Stl.CommandR;
using Stl.CommandR.Configuration;
using Stl.Fusion;

namespace HelloBlazorHybrid.Abstractions
{
    public interface IChatService
    {
        public record PostCommand(string Name, string Message) : ICommand<Unit>
        {
            // Default constructor is needed for JSON deserialization
            public PostCommand() : this(null!, null!) { }
        }

        [CommandHandler]
        Task PostMessageAsync(PostCommand command, CancellationToken cancellationToken = default);

        [ComputeMethod]
        Task<int> GetMessageCountAsync();

        [ComputeMethod]
        Task<(DateTime Time, string Name, string Message)[]> GetMessagesAsync(
            int count, CancellationToken cancellationToken = default);

        [ComputeMethod]
        Task<Unit> GetAnyTailAsync();
    }
}