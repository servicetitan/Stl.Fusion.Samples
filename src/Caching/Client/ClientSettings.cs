namespace Samples.Caching.Client;

public class ClientSettings
{
    public string ServerHost { get; set; } = "127.0.0.1";
    public int ServerPort { get; set; } = 5010;

    public Uri BaseUri => new($"http://{ServerHost}:{ServerPort}/");
    public Uri ApiBaseUri => new($"{BaseUri}api/");
}
