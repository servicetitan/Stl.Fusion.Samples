using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Stl.Fusion.Client;
using Samples.Gomoku.Abstractions;

namespace Samples.Gomoku.UI.Services
{
    [RestEaseReplicaService(typeof(ITimeService), Scope = Program.ClientSideScope)]
    [BasePath("time")]
    public interface ITimeClient
    {
        [Get("get")]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
    }
}
