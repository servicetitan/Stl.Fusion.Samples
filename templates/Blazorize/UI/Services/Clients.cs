using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Template.Blazorize.Abstractions;
using Stl.Fusion.Client;
using Stl.Serialization;

namespace Template.Blazorize.UI.Services
{
    [RestEaseReplicaService(typeof(ITranscriber), Scope = Program.ClientSideScope)]
    [BasePath("transcriber")]
    public interface ITranscriberClient
    {
        // Write API

        [Post("begin")]
        Task<string> BeginAsync([Body] Base64Data data, CancellationToken cancellationToken = default);
        [Post("append")]
        Task AppendAsync(string transcriptId, [Body] Base64Data data, CancellationToken cancellationToken = default);
        [Post("end")]
        Task EndAsync(string transcriptId, CancellationToken cancellationToken = default);

        // Read API

        [Get("get")]
        Task<Transcript> GetAsync(string transcriptId, CancellationToken cancellationToken = default);
        [Get("getSegmentCount")]
        Task<int?> GetSegmentCountAsync(string transcriptId, CancellationToken cancellationToken = default);
        [Get("getActiveTranscriptionIds")]
        Task<string[]> GetActiveTranscriptionIdsAsync(CancellationToken cancellationToken = default);
    }
}
