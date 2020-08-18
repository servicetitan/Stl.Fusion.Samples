using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;
using Stl.OS;
using Samples.Blazor.Common.Services;

namespace Samples.Blazor.Server.Services
{
    [ComputeService(typeof(IScreenshotService))]
    public class ScreenshotService : IScreenshotService
    {
        public const int MinWidth = 8;
        public const int MaxWidth = 1280;

        private readonly ImageCodecInfo _jpegEncoder;
        private readonly EncoderParameters _jpegEncoderParameters;
        private readonly Rectangle _displayDimensions;
        private Task<(Screenshot, Bitmap)> _next = null!;

        public ScreenshotService()
        {
            _jpegEncoder = ImageCodecInfo
                .GetImageDecoders()
                .Single(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
            _jpegEncoderParameters = new EncoderParameters(1) {
                Param = new [] {new EncoderParameter(Encoder.Quality, 50L)}
            };
            _displayDimensions = DisplayInfo.PrimaryDisplayDimensions
                ?? new Rectangle(0, 0, 1920, 1080);
        }

        public virtual async Task<Screenshot> GetScreenshotAsync(int width, CancellationToken cancellationToken = default)
        {
            var (screenshot, bitmap) = await GetScreenshotAsync(cancellationToken).ConfigureAwait(false);
            var w = Math.Min(MaxWidth, Math.Max(MinWidth, width));
            var h = w * screenshot.Height / screenshot.Width;
            if (screenshot.Width == w)
                return screenshot;

            // The code below scales a full-resolution screenshot to a desirable resolution
            using var bOut = new Bitmap(w, h);
            Scale(bitmap, bOut);
            return Encode(bOut);
        }

        [ComputeMethod(KeepAliveTime = 0.1, AutoInvalidateTime = 0.05)]
        protected virtual Task<(Screenshot, Bitmap)> GetScreenshotAsync(CancellationToken cancellationToken = default)
        {
            // Captures a full-resolution screenshot; the code here is optimized
            // to produce the next screeenshot in advance.
            Task<(Screenshot, Bitmap)> Capture() => Task.Run(() => {
                var (w, h) = (_displayDimensions.Width, _displayDimensions.Height);
                using var bScreen = new Bitmap(w, h);
                using var gScreen = Graphics.FromImage(bScreen);
                gScreen.CopyFromScreen(0, 0, 0, 0, bScreen.Size);
                if (w > MaxWidth)
                    (h, w) = (MaxWidth * h / w, MaxWidth);
                var bOut = new Bitmap(w, h);
                Scale(bScreen, bOut);
                return (Encode(bOut), bOut);
            }, default);

            var current = Capture();
            var prev = Interlocked.Exchange(ref _next, current) ?? current;
            Computed.GetCurrent()!.Invalidated += c => Task.Delay(2000).ContinueWith(_ => {
                // Let's dispose these values in 2 seconds
                var computed = (IComputed<(Screenshot, Bitmap)>) c;
                if (computed.HasValue)
                    computed.Value.Item2.Dispose();
            });
            return prev;
        }

        private static void Scale(Bitmap source, Bitmap target)
        {
            using var g = Graphics.FromImage(target);
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.CompositingMode = CompositingMode.SourceCopy;
            g.DrawImage(source, 0, 0, target.Width, target.Height);
        }

        private Screenshot Encode(Bitmap bitmap)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, _jpegEncoder, _jpegEncoderParameters);
            var bytes = stream.ToArray();
            var base64Content = Convert.ToBase64String(bytes);
            return new Screenshot(bitmap.Width, bitmap.Height, base64Content);
        }
    }
}
