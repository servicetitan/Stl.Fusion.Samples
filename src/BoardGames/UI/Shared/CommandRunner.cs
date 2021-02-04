using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Stl.CommandR;
using Stl.DependencyInjection;

namespace Samples.BoardGames.UI.Shared
{
    [Service(Lifetime = ServiceLifetime.Transient)]
    public class CommandRunner
    {
        private static readonly MethodInfo StateHasChangedMethod =
            typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.Instance | BindingFlags.NonPublic)!;

        public ICommander Commander { get; }
        public Exception? Error { get; private set; }
        public ComponentBase? Component { get; set; }

        public CommandRunner(ICommander commander)
            => Commander = commander;

        public void ResetError() => SetError(null);
        public void SetError(Exception? error)
        {
            if (Error == error)
                return;
            Error = error;
            if (Component != null)
                StateHasChangedMethod.Invoke(Component, Array.Empty<object>());
        }

        public async Task CallAsync<TResult>(ICommand command, CancellationToken cancellationToken = default)
        {
            ResetError();
            try {
                await Commander.CallAsync(command, cancellationToken);
            }
            catch (Exception e) {
                SetError(e);
            }
        }

        public async Task<TResult> CallAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        {
            ResetError();
            try {
                return await Commander.CallAsync(command, cancellationToken);
            }
            catch (Exception e) {
                SetError(e);
                return default!;
            }
        }
    }
}
