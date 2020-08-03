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
        private readonly ImageCodecInfo _jpegEncoder;
        private readonly EncoderParameters _jpegEncoderParameters;
        private readonly Rectangle _displayDimensions;
        private volatile Task<Screenshot> _prevScreenshotTask;

        public ScreenshotService()
        {
            _jpegEncoder = ImageCodecInfo
                .GetImageDecoders()
                .Single(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
            _jpegEncoderParameters = new EncoderParameters(1) {
                Param = {[0] = new EncoderParameter(Encoder.Quality, 50L)}
            };
            _displayDimensions = DisplayInfo.PrimaryDisplayDimensions
                ?? new Rectangle(0, 0, 1920, 1080);
            _prevScreenshotTask = MakeScreenshotAsync(128);
        }

        [ComputeMethod(AutoInvalidateTime = 0.02)]
        public virtual Task<Screenshot> GetScreenshotAsync(int width, CancellationToken cancellationToken = default)
            => MakeScreenshotAsync(width);

        private Task<Screenshot> MakeScreenshotAsync(int width)
            => Task.Run(() => {
                var (w, h) = (_displayDimensions.Width, _displayDimensions.Height);
                using var bScreen = new Bitmap(w, h);
                using var gScreen = Graphics.FromImage(bScreen);
                gScreen.CopyFromScreen(0, 0, 0, 0, bScreen.Size);
                var ow = width;
                var oh = h * ow / w;
                using var bOut = new Bitmap(ow, oh);
                using var gOut = Graphics.FromImage(bOut);
                gOut.CompositingQuality = CompositingQuality.HighSpeed;
                gOut.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gOut.CompositingMode = CompositingMode.SourceCopy;
                gOut.DrawImage(bScreen, 0, 0, ow, oh);
                using var stream = new MemoryStream();
                bOut.Save(stream, _jpegEncoder, _jpegEncoderParameters);
                var bytes = stream.ToArray();
                var base64Content = Convert.ToBase64String(bytes);
                return new Screenshot(ow, oh, base64Content);
            }, CancellationToken.None);
    }
}
