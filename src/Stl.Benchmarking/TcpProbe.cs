using System.Net.Sockets;

namespace Stl.Benchmarking;

public static class TcpProbe
{
    public static async Task WhenReady(string url, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(url);
        var host = uri.Host;
        var port = uri.Port == 0 ? 80 : uri.Port;
        var i = 0;
        while (!IsPortOpen(host, port)) {
            if ((++i & 1) == 0)
                WriteLine($"Waiting for {host}:{port} to open...");
            await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        }
    }

    public static bool IsPortOpen(string host, int port)
    {
        try {
            using var client = new TcpClient(host, port);
            return true;
        }
        catch (SocketException) {
            return false;
        }
    }
}
