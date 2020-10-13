using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Samples.Caching.Server;
using Stl.DependencyInjection;
using static System.Console;

namespace Samples.Caching.Client
{
    [Service]
    public class ServiceChecker
    {
        public string SamplesDir { get; set; }
        public string ServerBinDir { get; set; }
        public DbSettings DbSettings { get; set; }
        public ClientSettings ClientSettings { get; set; }

        public ServiceChecker(DbSettings dbSettings, ClientSettings clientSettings)
        {
            DbSettings = dbSettings;
            ClientSettings = clientSettings;
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var binCfgPart = Regex.Match(baseDir, @"[\\/]bin[\\/]\w+[\\/]").Value;
            var cachingSampleDir = Path.GetFullPath(Path.Combine(baseDir, "../../../.."));
            SamplesDir = Path.GetFullPath(Path.Combine(cachingSampleDir, "../.."));
            ServerBinDir = Path.GetFullPath(Path.Combine(cachingSampleDir, $"Server/{binCfgPart}/netcoreapp3.1/"));
        }

        public async Task WaitForServicesAsync(CancellationToken cancellationToken)
        {
            await WaitForServiceAsync("SQL Server", DbSettings.ServerHost, DbSettings.ServerPort,
                "docker-compose up -d db", cancellationToken);
            await WaitForServiceAsync("Samples.Caching.Server", ClientSettings.ServerHost, ClientSettings.ServerPort,
                "", cancellationToken);
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
