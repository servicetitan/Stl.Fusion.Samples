using Samples.Caching.Common;

namespace Samples.Caching.Server
{
    [Settings]
    public class ServerSettings : ISettings
    {
        public string SectionName => "Server";

        public string PublisherId { get; set; } = "p";
    }
}
