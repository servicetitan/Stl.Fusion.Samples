using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Client.Services
{
    public interface IScreenshotClient : IReplicaClient
    {
        [Get("get")]
        Task<Screenshot> GetScreenshotAsync(int width, CancellationToken cancellationToken = default);
    }

    public class ClientScreenshotService : IScreenshotService
    {
        private IScreenshotClient Client { get; }
        public ClientScreenshotService(IScreenshotClient client) => Client = client;

        public Task<Screenshot> GetScreenshotAsync(int width, CancellationToken cancellationToken)
            => Client.GetScreenshotAsync(width, cancellationToken);
    }
}
