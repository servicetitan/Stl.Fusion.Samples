using System;
using System.Text;
using Stl.DependencyInjection;

namespace Samples.Blazor.Server
{
    [Settings("Server")]
    public class ServerSettings
    {
        public string PublisherId { get; set; } = "p";

        public string MicrosoftAccountClientId { get; set; } = "c17d7c8e-de2c-42e3-9859-9437f03fb9a8";
        public string MicrosoftAccountClientSecret { get; set; } =
            Encoding.UTF8.GetString(Convert.FromBase64String(
                "c2pPR1VydmJkaDlVLn5VNk9Ycn41aFE3di1EeHpFNy50Qw=="));

        public string GitHubClientId { get; set; } = "7a38bc415f7e1200fee2";
        public string GitHubClientSecret { get; set; } =
            Encoding.UTF8.GetString(Convert.FromBase64String(
                "OGNkMTAzM2JmZjljOTk3ODc5MjhjNTNmMmE3Y2Q1NWU0ZmNlNjU0OA=="));
    }
}
