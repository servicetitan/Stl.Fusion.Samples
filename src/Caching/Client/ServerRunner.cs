using System;
using System.IO;
using System.Net.Sockets;
using System.Reactive;
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
        public Task<Unit> ReadyTask { get; }

        public ServerRunner()
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var binCfgPart = Regex.Match(baseDir, @"[\\/]bin[\\/]\w+[\\/]").Value;
            var cachingSampleDir = Path.GetFullPath(Path.Combine(baseDir, "../../../.."));
            SamplesDir = Path.GetFullPath(Path.Combine(cachingSampleDir, "../.."));
            ServerBinDir = Path.GetFullPath(Path.Combine(cachingSampleDir, $"Server/{binCfgPart}/netcoreapp3.1/"));
            ReadyTask = TaskSource.New<Unit>(true).Task;
        }

        protected override async Task RunInternalAsync(CancellationToken cancellationToken)
        {
            var outputTarget = PipeTarget.ToStream(Console.OpenStandardOutput());
            var startSqlServer = Cli.Wrap($"docker-compose{(OSInfo.Kind == OSKind.Windows ? ".exe" : "")}")
                .WithArguments("up -d db")
                .WithWorkingDirectory(SamplesDir)
                .WithStandardOutputPipe(outputTarget)
                .WithStandardErrorPipe(outputTarget);
            await startSqlServer.ExecuteAsync(cancellationToken);
            do {
                Console.WriteLine($"Waiting for SQL Server to start on {SqlServerIp}:{SqlServerPort}...");
                await Task.Delay(250, cancellationToken);
            } while (!IsPortOpen(SqlServerIp, SqlServerPort));

            var output = new StringBuilder();
            outputTarget = PipeTarget.Merge(
                PipeTarget.ToDelegate(s => {
                    lock (output) { output.Append(s); }
                }),
                PipeTarget.ToStream(Console.OpenStandardOutput()));
            var startServer = Cli.Wrap("dotnet")
                .WithArguments("Samples.Caching.Server.dll")
                .WithWorkingDirectory(ServerBinDir)
                .WithStandardOutputPipe(outputTarget)
                .WithStandardErrorPipe(outputTarget);
            ServerTask = startServer.ExecuteBufferedAsync(cancellationToken);
            for (;;) {
                await Task.Delay(100, cancellationToken);
                lock (output) {
                    if (output.ToString().Contains("Content root"))
                        break;
                }
            }
            TaskSource.For(ReadyTask).SetResult(default);

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
