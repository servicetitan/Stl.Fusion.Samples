using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestEase;

namespace Samples.Blazor.Server.Services
{
    [BasePath("https://api.forismatic.com/api/1.0/")]
    public interface IForismaticClient
    {
        [Get("?method=getQuote&format=json")]
        Task<JObject> GetQuote(
            [Query("lang")] string language = "en",
            CancellationToken cancellationToken = default);
    }
}
