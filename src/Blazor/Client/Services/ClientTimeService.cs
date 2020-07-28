using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Client.Services
{
    public interface ITimeClient : IReplicaClient
    {
        [Get("get")]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
    }

    public class ClientTimeService : ITimeService
    {
        private ITimeClient Client { get; }

        public ClientTimeService(ITimeClient client) => Client = client;

        public Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default)
            => Client.GetTimeAsync(cancellationToken);
    }
}
