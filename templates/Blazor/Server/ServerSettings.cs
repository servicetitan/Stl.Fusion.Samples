using System;
using System.Text;
using Stl.DependencyInjection;

namespace Templates.Blazor.Server
{
    [Settings("Server")]
    public class ServerSettings
    {
        public string PublishedId { get; init; } = "p";
        public string GitHubClientId { get; init; } = "7a38bc415f7e1200fee2";
        public string GitHubClientSecret { get; init; } =
            Encoding.UTF8.GetString(Convert.FromBase64String(
                "OGNkMTAzM2JmZjljOTk3ODc5MjhjNTNmMmE3Y2Q1NWU0ZmNlNjU0OA=="));
    }
}
