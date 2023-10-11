using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Samples.Blazor.Server.Services;

#pragma warning disable CA1416

public sealed class DirectBitmap : IDisposable
{
    private int _isDisposed;
    private GCHandle _gcHandle;
    public Image<Bgra32> Image { get; }
    public Bgra32[] Buffer { get; }
    public int Height { get; }
    public int Width { get; }

    public DirectBitmap(int width, int height)
    {
        if (width < 1)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 1)
            throw new ArgumentOutOfRangeException(nameof(height));
        Width = width;
        Height = height;
        Buffer = GC.AllocateUninitializedArray<Bgra32>(width * height, true);
        _gcHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
        Image = SixLabors.ImageSharp.Image.WrapMemory(Buffer.AsMemory(), width, height);
    }

    ~DirectBitmap() => Dispose();

    public void Dispose()
    {
        if (0 != Interlocked.Exchange(ref _isDisposed, 1))
            return;

        Image.Dispose();
        _gcHandle.Free();
        GC.SuppressFinalize(this);
    }
}
