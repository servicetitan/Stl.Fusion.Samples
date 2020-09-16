using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Stl;
using Stl.DependencyInjection;
using Stl.Generators;

namespace Samples.Blazor.Server.Services
{
    public interface IClientIdProvider
    {
        public string ClientId { get; }
        ValueTask ReadOrCreateAsync(HttpContext httpContext);
    }

    [Service(typeof(IClientIdProvider), Lifetime = ServiceLifetime.Scoped)]
    public class ClientIdProvider : IClientIdProvider
    {
        protected static Generator<string> Generator { get; set; } = RandomStringGenerator.Default;
        protected string SessionKey { get; set; } = nameof(ClientId);
        protected Result<string> ClientIdResult { get; set; }

        public string ClientId => ClientIdResult.Value;

        public ClientIdProvider()
            : this(Result.Error<string>(new InvalidOperationException("ClientId is undefined.")))
        { }

        public ClientIdProvider(Result<string> clientId) => ClientIdResult = clientId;

        public virtual async ValueTask ReadOrCreateAsync(HttpContext httpContext)
        {
            var session = httpContext.Session;
            await session.LoadAsync().ConfigureAwait(false);
            var clientId = session.GetString(SessionKey);
            if (string.IsNullOrEmpty(clientId)) {
                clientId = NewClientId();
                session.SetString(SessionKey, clientId);
            }
            ClientIdResult = clientId;
        }

        protected virtual string NewClientId() => Generator.Next();
    }
}
