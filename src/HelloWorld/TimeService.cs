using System;
using System.Threading;
using System.Threading.Tasks;
using Stl;
using Stl.Fusion;

namespace Samples.HelloWorld
{
    public class TimeService
    {
        [ComputeMethod]
        public virtual Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default)
        {
            var computed = Computed.GetCurrent();
            Console.WriteLine(computed);
            Task.Delay(2000).ContinueWith(_ => computed.Invalidate()).Ignore();
            return Task.FromResult(DateTime.Now);
        }
    }
}
