using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Client.Services
{
    public interface IComposerClient : IReplicaClient
    {
        [Get("get")]
        Task<ComposedValue> GetComposedValueAsync(string? parameter, CancellationToken cancellationToken = default);
    }

    public class ClientComposerService : IComposerService
    {
        private IComposerClient Client { get; }
        public ClientComposerService(IComposerClient client) => Client = client;

        public Task<ComposedValue> GetComposedValueAsync(string parameter, CancellationToken cancellationToken)
            => Client.GetComposedValueAsync(parameter, cancellationToken);
    }
}
