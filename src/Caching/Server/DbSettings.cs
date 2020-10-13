using Samples.Caching.Common;

namespace Samples.Caching.Server
{
    [Settings]
    public class DbSettings : ISettings
    {
        public string SectionName => "DB";

        public string ServerHost { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 5020;
        public string DatabaseName { get; set; } = "Samples_Caching";
    }
}
