using System.Runtime.CompilerServices;

namespace Samples.Blazor.UI.Services;

public sealed class FpsCounter(int frameCount = 15)
{
    private RingBuffer<CpuTimestamp> _timestamps = new(frameCount);

    public double Value {
        get {
            var frameCount = _timestamps.Count;
            if (frameCount < 2)
                return 0;

            return (frameCount - 1) / (_timestamps[frameCount - 1] - _timestamps[0]).TotalSeconds;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFrame()
        => AddFrame(CpuTimestamp.Now);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFrame(CpuTimestamp timestamp)
        => _timestamps.PushTailAndMoveHeadIfFull(timestamp);
}
