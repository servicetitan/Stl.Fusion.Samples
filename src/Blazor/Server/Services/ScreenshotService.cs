using System.IO;
using System.Reflection;
using Samples.Blazor.Abstractions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Stl.Rpc;
using Color = SixLabors.ImageSharp.Color;
using Image = SixLabors.ImageSharp.Image;
using PointF = SixLabors.ImageSharp.PointF;
using Point = SixLabors.ImageSharp.Point;

namespace Samples.Blazor.Server.Services;

#pragma warning disable CA1416

public class ScreenshotService : IScreenshotService
{
    private const int MinWidth = 8;
    private const int MaxWidth = 1280;
    private readonly CpuTimestamp _startedAt = CpuTimestamp.Now;
    private readonly Action<DirectBitmap, Stream> _jpegEncoder;
    private readonly JpegEncoder _unixJpegEncoder;
    private readonly FontCollection _fontCollection;
    private readonly Image<Bgra32> _sun;
    private Task<DirectBitmap>? _currentProducer;

    public ScreenshotService()
    {
        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var resourcesDir = Path.Combine(baseDir, "Resources");
        _fontCollection = new FontCollection();
        _fontCollection.Add($"{resourcesDir}/OpenSans-Bold.ttf");
        _fontCollection.Add($"{resourcesDir}/OpenSans-Regular.ttf");
        _sun = Image.Load<Bgra32>($"{resourcesDir}/Sun.jpg");

        _unixJpegEncoder = _unixJpegEncoder = new JpegEncoder() { Quality = 50 };
        _jpegEncoder = (source, stream) =>source.Image.Save(stream, _unixJpegEncoder);
    }

    public virtual async Task<RpcStream<Screenshot>> StreamScreenshots(int width, CancellationToken cancellationToken = default)
    {
        var cScreenshot0 = await Computed
            .Capture(() => GetScreenshot(width, cancellationToken))
            .ConfigureAwait(false);
        var screenshots = cScreenshot0
            .Changes(FixedDelayer.ZeroUnsafe, CancellationToken.None)
            .Select(c => c.Value);
        return new RpcStream<Screenshot>(screenshots) { AckPeriod = 5, AckAdvance = 11 };
    }

    public virtual async Task<Screenshot> GetScreenshot(int width, CancellationToken cancellationToken = default)
    {
        width = Math.Min(MaxWidth, Math.Max(MinWidth, width));
        var bitmap = await GetScreenshot(cancellationToken);
        return CreateScreenshotFromBitmap(bitmap, width);
    }

    [ComputeMethod(AutoInvalidationDelay = 1.0 / IScreenshotService.FrameRate)]
    protected virtual Task<DirectBitmap> GetScreenshot(CancellationToken cancellationToken = default)
    {
        // Captures a full-resolution screenshot; the code here is optimized
        // to produce the next screenshot in advance & instantly return the prev. one.
        var currentProducer = Task.Run(TakeScreenshot, default);
        var prevProducer = Interlocked.Exchange(ref _currentProducer, currentProducer) ?? currentProducer;
        Computed.GetCurrent()!.Invalidated += c => Task.Delay(1000).ContinueWith(_ => {
            // Let's dispose the bitmap in 1 second after invalidation
            var computed = (Computed<DirectBitmap>) c;
            if (computed.HasValue)
                computed.Value.Dispose();
        });
        return prevProducer;
    }

    private DirectBitmap TakeScreenshot()
    {
        var (w, h) = (1280, 720);
        var screen = new DirectBitmap(w, h);

        // Unix & Docker version renders the Sun, since screen capture doesn't work there
        var now = (CpuTimestamp.Now - _startedAt).TotalSeconds;

        PointF Wave(double xRate, double yRate, double offset = 0)
        {
            var (hw, hh) = (w / 2f, h / 2f);
            var x = hw + hw * (float)Math.Sin(now * xRate + offset);
            var y = hh + hh * (float)Math.Cos(now * yRate + offset);
            return new PointF(x, y);
        }

        Point SunWave(double xRate, double yRate, double offset = 0)
        {
            var (hw, hh) = ((_sun.Width - w - 1) / 2f, (_sun.Height - h - 1) / 2f);
            var x = hw + hw * (float)Math.Sin(now * xRate + offset);
            var y = hh + hh * (float)Math.Cos(now * yRate + offset);
            return new Point((int) -x, (int) -y);
        }

        var image = screen.Image;
        var font = _fontCollection.Get("Open Sans").CreateFont(48);
        var options = new DrawingOptions() {
            GraphicsOptions = { Antialias = true },
        };
        var time = DateTime.Now.ToString("HH:mm:ss.fff");
        image.Mutate(x => x
            .DrawImage(_sun, SunWave(0.01, 0.01), 1f)
            .DrawText(options, $"Time: {time}", font, Color.White, Wave(0.13, 0.17, -1)));
        return screen;
    }

    private Screenshot CreateScreenshotFromBitmap(DirectBitmap source, int width)
    {
        var height = width * source.Height / source.Width;
        using var stream = new MemoryStream(100000);
        if (source.Width == width)
            _jpegEncoder.Invoke(source, stream);
        else {
            using var iTarget = source.Image.Clone();
            iTarget.Mutate(x => x.Resize(width, height));
            iTarget.Save(stream, _unixJpegEncoder);
        }
        var frameOffset = CpuTimestamp.Now - _startedAt;
        return new Screenshot(source.Width, source.Height, stream.ToArray(), frameOffset);
    }
}
