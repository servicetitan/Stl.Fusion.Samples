using System;

namespace Template.Blazorize.Abstractions
{
    public class TranscriptSegment
    {
        public int Index { get; set; }
        public TimeSpan Offset { get; set; }
        public TimeSpan Duration { get; set; }
        public string Text { get; set; } = "";
        public string SpeakerId { get; set; } = "";

        public virtual bool IsSameAs(TranscriptSegment other)
            => Offset == other.Offset
            && Duration == other.Duration
            && Text == other.Text
            && SpeakerId == other.SpeakerId;
    }
}
