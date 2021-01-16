using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;
using Stl.OS;
using Samples.Blazor.Abstractions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;
using Image = SixLabors.ImageSharp.Image;
using PointF = SixLabors.ImageSharp.PointF;
using Point = SixLabors.ImageSharp.Point;

namespace Samples.Blazor.Server.Services
{
    [ComputeService(typeof(IScreenshotService))]
    public class ScreenshotService : IScreenshotService
    {
        private const int MinWidth = 8;
        private const int MaxWidth = 1280;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
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
            _fontCollection.Install($"{resourcesDir}/OpenSans-Bold.ttf");
            _fontCollection.Install($"{resourcesDir}/OpenSans-Regular.ttf");
            _sun = Image.Load<Bgra32>($"{resourcesDir}/Sun.jpg");

            _unixJpegEncoder = _unixJpegEncoder = new JpegEncoder() { Quality = 50 };
            _jpegEncoder = (source, stream) =>source.Image.Save(stream, _unixJpegEncoder);
            if (OSInfo.IsWindows) {
                var winJpegEncoder = ImageCodecInfo
                    .GetImageDecoders()
                    .Single(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
                var winJpegEncoderParameters = new EncoderParameters(1) {
                    Param = new [] {new EncoderParameter(Encoder.Quality, 50L)}
                };
                _jpegEncoder = (source, stream) =>
                    source.Bitmap.Save(stream, winJpegEncoder, winJpegEncoderParameters);
            }
        }

        public virtual async Task<Screenshot> GetScreenshotAsync(int width, CancellationToken cancellationToken = default)
        {
            width = Math.Min(MaxWidth, Math.Max(MinWidth, width));
            var bitmap = await GetScreenshotAsync(cancellationToken);
            return CreateScreenshot(bitmap, width);
        }

        [ComputeMethod(KeepAliveTime = 0.1, AutoInvalidateTime = 0.05)]
        protected virtual Task<DirectBitmap> GetScreenshotAsync(CancellationToken cancellationToken = default)
        {
            // Captures a full-resolution screenshot; the code here is optimized
            // to produce the next screenshot in advance & instantly return the prev. one.
            var currentProducer = Task.Run(TakeScreenshot, default);
            var prevProducer = Interlocked.Exchange(ref _currentProducer, currentProducer) ?? currentProducer;
            Computed.GetCurrent()!.Invalidated += c => Task.Delay(1000).ContinueWith(_ => {
                // Let's dispose the bitmap in 1 second after invalidation
                var computed = (IComputed<DirectBitmap>) c;
                if (computed.HasValue)
                    computed.Value.Dispose();
            });
            return prevProducer;
        }

        private DirectBitmap TakeScreenshot()
        {
            var (w, h) = (1280, 720);
            if (OSInfo.IsWindows) {
                var dd = DisplayInfo.PrimaryDisplayDimensions;
                (w, h) = (dd?.Width ?? 1280, dd?.Height ?? 720);
            }
            var screen = new DirectBitmap(w, h);
            if (OSInfo.IsWindows) {
                using var gScreen = Graphics.FromImage(screen.Bitmap);
                gScreen.CopyFromScreen(0, 0, 0, 0, screen.Bitmap.Size);
                return screen;
            }

            // Unix & Docker version renders the Sun, since screen capture doesn't work there
            var now = _stopwatch.Elapsed.TotalSeconds;

            PointF Wave(double xRate, double yRate, double offset = 0)
            {
                var (hw, hh) = (w / 2f, h / 2f);
                var x = hw + hw * (float) Math.Sin(now * xRate + offset);
                var y = hh + hh * (float) Math.Cos(now * yRate + offset);
                return new PointF(x, y);
            }

            Point SunWave(double xRate, double yRate, double offset = 0)
            {
                var (hw, hh) = ((_sun.Width - w - 1) / 2f, (_sun.Height - h - 1) / 2f);
                var x = hw + hw * (float) Math.Sin(now * xRate + offset);
                var y = hh + hh * (float) Math.Cos(now * yRate + offset);
                return new Point((int) -x, (int) -y);
            }

            var image = screen.Image;
            var font = _fontCollection.Find("Open Sans").CreateFont(48);
            var options = new TextGraphicsOptions() {
                GraphicsOptions = { Antialias = true },
                TextOptions = {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                }
            };
            var time = DateTime.Now.ToString("HH:mm:ss.fff");
            image.Mutate(x => x
                .DrawImage(_sun, SunWave(0.01, 0.01), 1f)
                .DrawText(options, $"Time: {time}", font, Color.White, Wave(0.13, 0.17, -1)));
            return screen;
        }

        private Screenshot CreateScreenshot(DirectBitmap source, int width)
        {
            var height = width * source.Height / source.Width;
            using var stream = new MemoryStream(100000);
            if (source.Width == width)
                _jpegEncoder.Invoke(source, stream);
            else if (OSInfo.IsWindows) {
                var target = new DirectBitmap(width, height);
                using var gTarget = Graphics.FromImage(target.Bitmap);
                gTarget.CompositingQuality = CompositingQuality.HighSpeed;
                gTarget.InterpolationMode = InterpolationMode.Bilinear;
                gTarget.CompositingMode = CompositingMode.SourceCopy;
                gTarget.DrawImage(source.Bitmap, 0, 0, target.Width, target.Height);
                _jpegEncoder.Invoke(target, stream);
            }
            else {
                using var iTarget = source.Image.Clone();
                iTarget.Mutate(x => x.Resize(width, height));
                iTarget.Save(stream, _unixJpegEncoder);
            }
            var bytes = stream.ToArray();
            var base64Content = Convert.ToBase64String(bytes);
            return new Screenshot(source.Width, source.Height, base64Content);
        }
    }
}
