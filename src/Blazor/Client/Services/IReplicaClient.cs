using RestEase;
using Stl.Fusion.Bridge;
using Stl.Fusion.Client;

namespace Samples.Blazor.Client.Services
{
    // The sole purpose of this interface is to
    // DRY the [Header] attribute
    [Header(FusionHeaders.RequestPublication, "1")]
    public interface IReplicaClient : IReplicaService { }
}
