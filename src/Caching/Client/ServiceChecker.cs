using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace Samples.Caching.Client
{
    public class ServiceChecker
    {
        public string SamplesDir { get; set; }
        public string ServerBinDir { get; set; }

        public ServiceChecker()
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var binCfgPart = Regex.Match(baseDir, @"[\\/]bin[\\/]\w+[\\/]").Value;
            var cachingSampleDir = Path.GetFullPath(Path.Combine(baseDir, "../../../.."));
            SamplesDir = Path.GetFullPath(Path.Combine(cachingSampleDir, "../.."));
            ServerBinDir = Path.GetFullPath(Path.Combine(cachingSampleDir, $"Server/{binCfgPart}/netcoreapp3.1/"));
        }

        public async Task WaitForServicesAsync(CancellationToken cancellationToken)
        {
            await WaitForServiceAsync("SQL Server", "127.0.0.1", 5020, "docker-compose up -d db", cancellationToken);
            await WaitForServiceAsync("Samples.Caching.Server", "127.0.0.1", 5010, "", cancellationToken);
        }

        private async Task WaitForServiceAsync(string name, string ipAddress, int port, string command, CancellationToken cancellationToken)
        {
            var helpDisplayed = false;
            while (!IsPortOpen(ipAddress, port)) {
                if (!helpDisplayed) {
                    helpDisplayed = true;
                    if (string.IsNullOrEmpty(command))
                        WriteLine($"Start {name}.");
                    else {
                        WriteLine($"Start {name} by running:");
                        WriteLine($"  {command}");
                    }
                }
                WriteLine($"Waiting for {name} to open {ipAddress}:{port}...");
                await Task.Delay(1000, cancellationToken);
            }
        }

        private static bool IsPortOpen(string url, int port)
        {
            try {
                using var client = new TcpClient(url, port);
                return true;
            }
            catch (SocketException) {
                return false;
            }
        }
    }
}
