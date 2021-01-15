using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stl.Async;
using Stl.CommandR;
using Stl.CommandR.Configuration;
using Stl.DependencyInjection;
using Stl.Fusion.Operations;

namespace Samples.HelloBlazorServer.Services
{
    [CommandService, AddHostedService]
    public class ChatBotService : IHostedService
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

        public ChatBotService(ChatService chatService)
            => _chatService = chatService;

        public async Task StartAsync(CancellationToken cancellationToken)
            => await _chatService.PostMessageAsync(new(Morpheus, MorpheusMessage1), cancellationToken);
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        // 100 is priority of invalidation handler, and this handler has to run before it
        // to avoid being wrapped into Compute.Invalidated() scope.
        [CommandHandler(Priority = 200, IsFilter = true)]
        protected virtual async Task OnChatPost(ICompletion<ChatService.PostCommand> completion, CancellationToken cancellationToken)
        {
            // Let the rest of the chain proceed
            await CommandContext.GetCurrent().InvokeRemainingHandlersAsync(cancellationToken);
            // And start the reaction - no need to delay the rest of command processing pipeline
            Task.Run(() => Reaction(completion, default), default).Ignore();
        }

        protected virtual async Task Reaction(ICompletion<ChatService.PostCommand> completion, CancellationToken cancellationToken)
        {
            var messageCount = await _chatService.GetMessageCountAsync();
            switch (messageCount) {
            case 1:
                break;
            case 2:
                await Task.Delay(1000);
                await _chatService.PostMessageAsync(new(Morpheus, MorpheusMessage2));
                break;
            default:
                var messages = await _chatService.GetMessagesAsync(1, cancellationToken);
                var (time, name, message) = messages.SingleOrDefault();
                name ??= "";
                if (name == "" || BotNames.Contains(name))
                    break;
                if (message.ToLowerInvariant().Contains("time"))
                    await _chatService.PostMessageAsync(new(TimeBot, DateTime.Now.ToString("F")));
                else
                    await _chatService.PostMessageAsync(new(Groot, GrootMessage));
                break;
            }
        }
    }
}
