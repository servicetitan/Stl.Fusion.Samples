using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Templates.Blazor.Common.Services;
using Stl.Fusion.Client;

namespace Templates.Blazor.Client.Services
{
    [RestEaseReplicaService(typeof(ITimeService), Scope = Program.ClientSideScope)]
    [BasePath("time")]
    public interface ITimeClient
    {
        [Get("get")]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
    }
}
