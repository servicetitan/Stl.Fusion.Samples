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
    public static class Part01
    {
        #region Part01_CreateServices
        public static IServiceProvider CreateServices()
        {
            var services = new ServiceCollection();
            services.AddFusion();
            services.AttributeBased().AddServicesFrom(Assembly.GetExecutingAssembly());
            return services.BuildServiceProvider();
        }
        #endregion

        #region Part01_CounterService
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
        #endregion

        public static async Task UseCounterService1()
        {
            #region Part01_UseCounterService1
            var counters = CreateServices().GetRequiredService<CounterService>();
            WriteLine(await counters.GetAsync("a"));
            WriteLine(await counters.GetAsync("b"));
            #endregion
        }

        public static async Task UseCounterService2()
        {
            #region Part01_UseCounterService2
            var counters = CreateServices().GetRequiredService<CounterService>();
            WriteLine(await counters.GetAsync("a"));
            WriteLine(await counters.GetAsync("a"));
            #endregion
        }

        public static async Task UseCounterService3()
        {
            #region Part01_UseCounterService3
            var counters = CreateServices().GetRequiredService<CounterService>();
            WriteLine(await counters.GetAsync("a"));
            counters.Increment("a");
            WriteLine(await counters.GetAsync("a"));
            #endregion
        }

        #region Part01_CounterSumService
        [ComputeService] // You don't need this attribute if you manually register such services
        public class CounterSumService
        {
            public CounterService Counters { get; }

            public CounterSumService(CounterService counters) => Counters = counters;

            [ComputeMethod]
            public virtual async Task<int> SumAsync(string key1, string key2)
            {
                WriteLine($"{nameof(SumAsync)}({key1}, {key2})");
                return await Counters.GetAsync(key1) + await Counters.GetAsync(key2);
            }
        }
        #endregion

        public static async Task UseCounterSumService1()
        {
            #region Part01_UseCounterSumService1
            var services = CreateServices();
            var counterSum = services.GetRequiredService<CounterSumService>();
            WriteLine(await counterSum.SumAsync("a", "b"));
            WriteLine(await counterSum.SumAsync("a", "b"));
            #endregion
        }

        public static async Task UseCounterSumService2()
        {
            #region Part01_UseCounterSumService2
            var services = CreateServices();
            var counterSum = services.GetRequiredService<CounterSumService>();
            WriteLine("Nothing is cached (yet):");
            WriteLine(await counterSum.SumAsync("a", "b"));
            WriteLine("Only GetAsync(a) and GetAsync(b) outputs are cached:");
            WriteLine(await counterSum.SumAsync("b", "a"));
            WriteLine("Everything is cached:");
            WriteLine(await counterSum.SumAsync("a", "b"));
            #endregion
        }

        public static async Task UseCounterSumService3()
        {
            #region Part01_UseCounterSumService3
            var services = CreateServices();
            var counters = services.GetRequiredService<CounterService>();
            var counterSum = services.GetRequiredService<CounterSumService>();
            WriteLine(await counterSum.SumAsync("a", "b"));
            counters.Increment("a");
            WriteLine(await counterSum.SumAsync("a", "b"));
            #endregion
        }

        #region Part01_HelloService
        [ComputeService] // You don't need this attribute if you manually register such services
        public class HelloService
        {
            [ComputeMethod]
            public virtual async Task<string> HelloAsync(string name)
            {
                WriteLine($"+ {nameof(HelloAsync)}({name})");
                await Task.Delay(1000);
                WriteLine($"- {nameof(HelloAsync)}({name})");
                return $"Hello, {name}!";
            }
        }
        #endregion

        public static async Task UseHelloService1()
        {
            #region Part01_UseHelloService1
            var hello = CreateServices().GetRequiredService<HelloService>();
            var t1 = Task.Run(() => hello.HelloAsync("Alice"));
            var t2 = Task.Run(() => hello.HelloAsync("Bob"));
            var t3 = Task.Run(() => hello.HelloAsync("Bob"));
            var t4 = Task.Run(() => hello.HelloAsync("Alice"));
            await Task.WhenAll(t1, t2, t3, t4);
            WriteLine(t1.Result);
            WriteLine(t2.Result);
            WriteLine(t3.Result);
            WriteLine(t4.Result);
            #endregion
        }
    }
}
