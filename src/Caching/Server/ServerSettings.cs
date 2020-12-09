using Stl.DependencyInjection;

namespace Samples.Caching.Server
{
    [Settings("Server")]
    public class ServerSettings
    {
        public string PublisherId { get; set; } = "p";
    }
}
