using System.Net.Sockets;
using static System.Console;

namespace Samples.Benchmark.Client;

public static class ServerChecker
{
    public static async Task WhenReady(string url, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(url);
        var host = uri.Host;
        var port = uri.Port == 0 ? 80 : uri.Port;
        while (!IsPortOpen(host, port)) {
            WriteLine($"Waiting for {host}:{port} to open...");
            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        }
        await Task.Delay(500, cancellationToken).ConfigureAwait(false);
    }

    private static bool IsPortOpen(string host, int port)
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
