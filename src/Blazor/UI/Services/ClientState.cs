using System;
using Microsoft.Extensions.DependencyInjection;
using Samples.Blazor.Abstractions;
using Stl;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.Blazor;

namespace Samples.Blazor.UI.Services
{
    [Service(Lifetime = ServiceLifetime.Scoped)]
    public class ClientState : IDisposable
    {
        protected AuthStateProvider AuthStateProvider { get; }
        protected ISessionResolver SessionResolver { get; }

        // Handy shortcuts
        public Session Session => SessionResolver.Session;
        public ILiveState<AuthState> AuthState => AuthStateProvider.State;
        // Own properties
        public ILiveState<User> User { get; }
        // public IMutableState<ChatUser?> ChatUser { get; }
        public IMutableState<Board?> Board { get; }

        public ClientState(AuthStateProvider authStateProvider, IStateFactory stateFactory)
        {
            AuthStateProvider = authStateProvider;
            SessionResolver = AuthStateProvider.SessionResolver;

            User = stateFactory.NewLive<User>(
                o => o.WithUpdateDelayer(0, 1),
                async (_, cancellationToken) => {
                    var authState = await AuthState.Use(cancellationToken).ConfigureAwait(false);
                    return authState.User;
                });
            // ChatUser = stateFactory.NewMutable(Result.Value<ChatUser?>(null));
            Board = stateFactory.NewMutable(Result.Value<Board?>(null));
        }

        void IDisposable.Dispose() => User.Dispose();
    }
}