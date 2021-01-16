using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Samples.Blazor.Abstractions;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.Blazor;

namespace Samples.Blazor.UI.Services
{
    [Service(Lifetime = ServiceLifetime.Scoped)]
    public class ClientState : IDisposable
    {
        protected IChatService ChatService { get; }
        protected AuthStateProvider AuthStateProvider { get; }
        protected ISessionResolver SessionResolver { get; }

        // Handy shortcuts
        public Session Session => SessionResolver.Session;
        public ILiveState<AuthState> AuthState => AuthStateProvider.State;
        // Own properties
        public ILiveState<User> User { get; }
        public ILiveState<ChatUser?> ChatUser { get; }

        public ClientState(
            IChatService chatService,
            AuthStateProvider authStateProvider,
            IStateFactory stateFactory)
        {
            ChatService = chatService;
            AuthStateProvider = authStateProvider;
            SessionResolver = AuthStateProvider.SessionResolver;

            User = stateFactory.NewLive<User>(
                o => o.WithInstantUpdates(),
                async (_, cancellationToken) => {
                    var authState = await AuthState.UseAsync(cancellationToken);
                    return authState.User;
                });
            ChatUser = stateFactory.NewLive<ChatUser?>(
                o => {
                    o.InitialOutputFactory = _ => null; // The default factory uses parameterless constructor instead
                    o.WithInstantUpdates();
                },
                (_, cancellationToken) => ChatService.GetCurrentUserAsync(Session, cancellationToken));
        }

        void IDisposable.Dispose() => User.Dispose();
    }
}
