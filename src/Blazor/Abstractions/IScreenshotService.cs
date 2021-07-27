using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.Blazor.Abstractions
{
    public class Screenshot
    {
        private static readonly string OnePixelBase64Content =
            "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAUDBAQEAwUEBAQFBQUGBwwIBwcHBw8LCwkMEQ8SEhEPERETFhwXExQaFRERGCEYGh0d" +
            "Hx8fExciJCIeJBweHx7/2wBDAQUFBQcGBw4ICA4eFBEUHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4e" +
            "Hh4eHh4eHh7/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAj/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QA" +
            "FAEBAAAAAAAAAAAAAAAAAAAAAP/EABQRAQAAAAAAAAAAAAAAAAAAAAD/2gAMAwEAAhEDEQA/ALLAB//Z";

        public int Width { get; } = 1;
        public int Height { get; } = 1;
        public string Base64Content { get; } = "";

        public Screenshot() => Base64Content = OnePixelBase64Content;

        [JsonConstructor]
        public Screenshot(int width, int height, string base64Content)
        {
            Width = width;
            Height = height;
            Base64Content = base64Content;
        }
    }

    public interface IScreenshotService
    {
        [ComputeMethod(KeepAliveTime = 0.1)]
        Task<Screenshot> GetScreenshot(int width, CancellationToken cancellationToken = default);
    }
}
