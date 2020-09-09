using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stl.DependencyInjection;
using Stl.Fusion;

namespace Samples.HelloBlazorServer.Services
{
    [HostedService]
    public class ChatBotService : IHostedService, IDisposable
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
        private static readonly HashSet<string> BotNames = new HashSet<string>() {Morpheus, Groot, TimeBot};

        private readonly ChatService _chatService;
        private readonly ILiveState<(DateTime Time, string Name, string Message)[]> _state;

        public ChatBotService(ChatService chatService, IStateFactory stateFactory)
        {
            _chatService = chatService;
            _state = stateFactory.NewLive<(DateTime Time, string Name, string Message)[]>(
                options => {
                    options.WithZeroUpdateDelay();
                    options.Updated = _ => Task.Run(TryRespondAsync);
                },
                (state, cancellationToken) => _chatService.GetMessagesAsync(5, cancellationToken));
        }

        public void Dispose() => _state.Dispose();
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task TryRespondAsync()
        {
            var messages = _state?.Value;
            if (messages == null) // The initial update
                return;
            switch (messages.Length) {
            case 0:
                await _chatService.PostMessageAsync(Morpheus, MorpheusMessage1).ConfigureAwait(false);
                break;
            case 1:
                break;
            case 2:
                await Task.Delay(1000).ConfigureAwait(false);
                await _chatService.PostMessageAsync(Morpheus, MorpheusMessage2).ConfigureAwait(false);
                break;
            default:
                var (time, name, message) = messages.LastOrDefault();
                if (name == null || BotNames.Contains(name))
                    break;
                if (message.ToLowerInvariant().Contains("time"))
                    await _chatService.PostMessageAsync(TimeBot, DateTime.Now.ToString("F"));
                else
                    await _chatService.PostMessageAsync(Groot, GrootMessage).ConfigureAwait(false);
                break;
            }
        }
    }
}
