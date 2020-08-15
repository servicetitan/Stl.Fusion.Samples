using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Stl.Fusion.Client;

namespace Samples.Blazor.Server.Services
{
    [RestEaseService]
    [BasePath("https://uzby.com/api.php")]
    public interface IUzbyClient
    {
        [Get("")]
        Task<string> GetNameAsync(
            [Query("min")] int minLength = 2,
            [Query("max")] int maxLength = 8,
            CancellationToken cancellationToken = default);
    }
}
