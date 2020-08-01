using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion.UI;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Client.Models
{
    public class ChatState
    {
        public long UserCount { get; set; } = 0;
        public long ActiveUserCount { get; set; } = 0;
        public ChatPage LastPage { get; set; } = new ChatPage();

        public class Local
        {
            // It's global to the app, so we store it in static field
            private static volatile ChatUser? _me;

            public ChatUser? Me {
                get => _me;
                set {
                    _me = value;
                    if (string.IsNullOrEmpty(MyName) && value != null)
                        MyName = value.Name;
                }
            }

            public string MyName { get; set; } = "";
            public string MyMessage { get; set; } = "";
            public Exception? Error { get; set; }

            public Local Clone()
                => (Local) MemberwiseClone();
        }

        [LiveStateUpdater]
        public class Updater : ILiveStateUpdater<Local, ChatState>
        {
            protected IChatService Chat { get; }

            public Updater(IChatService chat) => Chat = chat;

            public virtual async Task<ChatState> UpdateAsync(
                ILiveState<Local, ChatState> liveState, CancellationToken cancellationToken)
            {
                var userCount = await Chat.GetUserCountAsync(cancellationToken);
                var activeUserCount = await Chat.GetActiveUserCountAsync(cancellationToken);
                var lastPage = await Chat.GetChatTailAsync(30, cancellationToken);
                var state = new ChatState() {
                    UserCount = userCount,
                    ActiveUserCount = activeUserCount,
                    LastPage = lastPage,
                };
                return state;
            }
        }
    }

}
