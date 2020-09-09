using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Stl.Async;
using Stl.Fusion;

namespace Samples.HelloBlazorServer.Services
{
    [ComputeService]
    public class ChatService
    {
        private volatile ImmutableList<(DateTime Time, string Name, string Message)> _messages =
            ImmutableList<(DateTime, string, string)>.Empty;

        public virtual Task PostMessageAsync(string name, string message)
        {
            // Lock-free update
            var spinWait = new SpinWait();
            for (;;) {
                var oldMessages = _messages;
                var newMessages = oldMessages!.Add((DateTime.Now, name, message));
                if (oldMessages == Interlocked.CompareExchange(ref _messages, newMessages, oldMessages))
                    break;
                spinWait.SpinOnce();
            }
            Computed.Invalidate(GetAnyMessagesAsync);
            return Task.CompletedTask;
        }

        [ComputeMethod]
        public virtual async Task<(DateTime Time, string Name, string Message)[]> GetMessagesAsync(
            int count, CancellationToken cancellationToken = default)
        {
            // Fake dependency used to invalidate all GetMessagesAsync(...) independently on count argument
            await GetAnyMessagesAsync().ConfigureAwait(false);
            return _messages.TakeLast(count).ToArray();
        }

        [ComputeMethod]
        protected virtual Task<Unit> GetAnyMessagesAsync() => TaskEx.UnitTask;
    }
}
