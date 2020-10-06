using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Stl.Async;
using Stl.OS;

namespace Samples.Caching.Client
{
    public class ServerRunner : AsyncProcessBase
    {
        public string SamplesDir { get; set; }
        public string ServerBinDir { get; set; }
        public string SqlServerIp { get; set; } = "127.0.0.1";
        public int SqlServerPort { get; set; } = 5020;
        public CommandTask<BufferedCommandResult>? ServerTask { get; private set; }

        public ServerRunner()
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var binCfgPart = Regex.Match(baseDir, @"[\\/]bin[\\/]\w+[\\/]").Value;
            var cachingSampleDir = Path.GetFullPath(Path.Combine(baseDir, "../../../.."));
            SamplesDir = Path.GetFullPath(Path.Combine(cachingSampleDir, "../.."));
            ServerBinDir = Path.GetFullPath(Path.Combine(cachingSampleDir, $"Server/{binCfgPart}/netcoreapp3.1/"));
        }

        protected override async Task RunInternalAsync(CancellationToken cancellationToken)
        {
            var startSqlServer = Cli.Wrap($"docker-compose{(OSInfo.Kind == OSKind.Windows ? ".exe" : "")}")
                .WithArguments("up -d db")
                .WithWorkingDirectory(SamplesDir)
                .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
                .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()));
            await startSqlServer.ExecuteAsync(cancellationToken);
            do {
                Console.WriteLine($"Waiting for SQL Server to start on {SqlServerIp}:{SqlServerPort}...");
                await Task.Delay(250, cancellationToken).ConfigureAwait(false);
            } while (!IsPortOpen(SqlServerIp, SqlServerPort));

            var startServer = Cli.Wrap("dotnet")
                .WithArguments("Samples.Caching.Server.dll")
                .WithWorkingDirectory(ServerBinDir)
                .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
                .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()));
            ServerTask = startServer.ExecuteBufferedAsync(cancellationToken);
            await ServerTask;
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
