using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestEase;
using Stl.Fusion.Client;

namespace Samples.Blazor.Server.Services
{
    [RestEaseClientService]
    [BasePath("https://api.forismatic.com/api/1.0/")]
    public interface IForismaticClient
    {
        [Get("?method=getQuote&format=json")]
        Task<JObject> GetQuoteAsync(
            [Query("lang")] string language = "en",
            CancellationToken cancellationToken = default);
    }
}
