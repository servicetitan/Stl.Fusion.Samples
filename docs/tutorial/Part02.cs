using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stl.DependencyInjection;
using Stl.Fusion;
using static System.Console;

namespace Tutorial
{
    public static class Part02
    {
        #region Part02_CounterService
        [ComputeService] // You don't need this attribute if you manually register such services
        public class CounterService
        {
            private readonly ConcurrentDictionary<string, int> _counters = new ConcurrentDictionary<string, int>();

            [ComputeMethod]
            public virtual async Task<int> GetAsync(string key)
            {
                WriteLine($"{nameof(GetAsync)}({key})");
                return _counters.TryGetValue(key, out var value) ? value : 0;
            }

            public void Increment(string key)
            {
                WriteLine($"{nameof(Increment)}({key})");
                _counters.AddOrUpdate(key, k => 1, (k, v) => v + 1);
                Computed.Invalidate(() => GetAsync(key));
            }
        }

        public static IServiceProvider CreateServices()
            => new ServiceCollection()
                .AddFusionCore()
                .AddDiscoveredServices(Assembly.GetExecutingAssembly())
                .BuildServiceProvider();
        #endregion

        public static async Task CaptureComputed()
        {
            #region Part02_CaptureComputed
            var counters = CreateServices().GetService<CounterService>();
            var computed = await Computed.CaptureAsync(_ => counters.GetAsync("a"));
            WriteLine($"Computed: {computed}");
            WriteLine($"- IsConsistent(): {computed.IsConsistent()}");
            WriteLine($"- Value:          {computed.Value}");
            #endregion
        }
    }
}
