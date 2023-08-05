namespace Samples.Blazor.Server;

public class ServerSettings
{
    public bool AssumeHttps { get; set; } = false;

    public string MicrosoftAccountClientId { get; set; } = "6839dbf7-d1d3-4eb2-a7e1-ce8d48f34d00";
    public string MicrosoftAccountClientSecret { get; set; } =
        Encoding.UTF8.GetString(Convert.FromBase64String(
            "YnlCOFF+QkdyUk9VcUlQWHdrYWxFQVNrQmpoaC0zdGdFU1JncGNMVA=="));

    public string GitHubClientId { get; set; } = "7a38bc415f7e1200fee2";
    public string GitHubClientSecret { get; set; } =
        Encoding.UTF8.GetString(Convert.FromBase64String(
            "OGNkMTAzM2JmZjljOTk3ODc5MjhjNTNmMmE3Y2Q1NWU0ZmNlNjU0OA=="));
}
