using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Stl.Fusion.Client;
using Samples.BoardGames.Abstractions;

namespace Samples.BoardGames.UI.Services
{
    [RestEaseReplicaService(typeof(IGameService), Scope = Program.ClientSideScope)]
    [BasePath("time")]
    public interface ITimeClient
    {
        [Get("get")]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
    }
}
