using System;
using System.Threading.Tasks;

namespace Samples.Caching.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await using var serverRunner = new ServerRunner();
            // ReSharper disable once AccessToDisposedClosure
            Console.CancelKeyPress += (s, ea) => serverRunner.Dispose();
            await serverRunner.RunAsync();
        }
    }
}
