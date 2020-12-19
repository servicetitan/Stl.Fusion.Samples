using System;
using System.Collections.Immutable;
using System.Linq;
using Stl;
using Stl.Collections;

namespace Template.Blazorize.Abstractions
{
    public class Transcript : IHasId<string>, IHasKey<string>
    {
        public string Id { get; }
        string IHasKey<string>.Key => Id;
        public ImmutableList<TranscriptSegment> Segments { get; set; }

        public DateTime StartTime { get; }
        public TimeSpan Duration {
            get {
                var lastSegment = Segments.Count > 0 ? Segments[^1] : null;
                if (lastSegment == null)
                    return TimeSpan.Zero;
                return lastSegment.Offset + lastSegment.Duration;
            }
        }

        public string Text => Segments.Select(s => s.Text).ToDelimitedString(" ");

        public Transcript(string id, DateTime startTime, ImmutableList<TranscriptSegment>? segments = null)
        {
            Id = id;
            StartTime = startTime;
            Segments = segments ?? ImmutableList<TranscriptSegment>.Empty;
        }

    }
}
