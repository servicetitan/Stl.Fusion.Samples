using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;
using Stl.Serialization;

namespace Template.Blazorize.Abstractions
{
    public interface ITranscriber
    {
        // Write API

        /// <summary>
        /// Begins audio stream transcription.
        /// </summary>
        /// <para>
        /// You should call <see cref="EndAsync"/> to ensure
        /// the resources allocated during this call are released as soon as possible,
        /// otherwise they'll be released in ~ 1 minute after the last <see cref="AppendAsync"/>
        /// or <see cref="BeginAsync"/> call.
        /// </para>
        /// <param name="data">The first data fragment to transcribe. Can be an empty array.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task with Transcription ID.</returns>
        Task<string> BeginAsync(Base64Data data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Transcribes the next data fragment of audio stream.
        /// </summary>
        /// <param name="transcriptId">Transcription ID.</param>
        /// <param name="data">The data fragment to transcribe. Can be an empty array.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task indicating the completion of transcription of this fragment.</returns>
        Task AppendAsync(string transcriptId, Base64Data data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Completed the transcription and releases all resources associated with it.
        /// </summary>
        /// <para>Once this method is called, all read endpoints for the specified
        /// <paramref name="transcriptId"/> will throw an exception.</para>
        /// <param name="transcriptId">Transcription ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task indicating the completion of transcription disposal.</returns>
        Task EndAsync(string transcriptId, CancellationToken cancellationToken = default);

        // Read API

        [ComputeMethod]
        Task<Transcript> GetAsync(string transcriptId, CancellationToken cancellationToken = default);
        [ComputeMethod]
        Task<int?> GetSegmentCountAsync(string transcriptId, CancellationToken cancellationToken = default);
        [ComputeMethod]
        Task<string[]> GetActiveTranscriptionIdsAsync(CancellationToken cancellationToken = default);
    }
}
