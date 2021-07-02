using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stl;
using Stl.Fusion;
using Stl.Fusion.Swapping;
using static System.Console;

namespace Tutorial
{
    public static class Part05
    {
        #region Part05_Service1
        public class Service1
        {
            [ComputeMethod]
            public virtual async Task<string> Get(string key)
            {
                WriteLine($"{nameof(Get)}({key})");
                return key;
            }
        }

        public static IServiceProvider CreateServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ISwapService, DemoSwapService>();
            services.AddFusion()
                .AddComputeService<Service1>()
                .AddComputeService<Service2>() // We'll use Service2 & other services later
                .AddComputeService<Service3>()
                .AddComputeService<Service4>();
            return services.BuildServiceProvider();
        }
        #endregion

        public static async Task Caching1()
        {
            #region Part05_Caching1
            var service = CreateServices().GetRequiredService<Service1>();
            // var computed = await Computed.Capture(_ => counters.GetAsync("a"));
            WriteLine(await service.Get("a"));
            WriteLine(await service.Get("a"));
            GC.Collect();
            WriteLine("GC.Collect()");
            WriteLine(await service.Get("a"));
            WriteLine(await service.Get("a"));
            #endregion
        }

        public static async Task Caching2()
        {
            #region Part05_Caching2
            var service = CreateServices().GetRequiredService<Service1>();
            var computed = await Computed.Capture(_ => service.Get("a"));
            WriteLine(await service.Get("a"));
            WriteLine(await service.Get("a"));
            GC.Collect();
            WriteLine("GC.Collect()");
            WriteLine(await service.Get("a"));
            WriteLine(await service.Get("a"));
            #endregion
        }

        #region Part05_Service2
        public class Service2
        {
            [ComputeMethod]
            public virtual async Task<string> Get(string key)
            {
                WriteLine($"{nameof(Get)}({key})");
                return key;
            }

            [ComputeMethod]
            public virtual async Task<string> Combine(string key1, string key2)
            {
                WriteLine($"{nameof(Combine)}({key1}, {key2})");
                return await Get(key1) + await Get(key2);
            }
        }
        #endregion

        public static async Task Caching3()
        {
            #region Part05_Caching3
            var service = CreateServices().GetRequiredService<Service2>();
            var computed = await Computed.Capture(_ => service.Combine("a", "b"));
            WriteLine("computed = CombineAsync(a, b) completed");
            WriteLine(await service.Combine("a", "b"));
            WriteLine(await service.Get("a"));
            WriteLine(await service.Get("b"));
            WriteLine(await service.Combine("a", "c"));
            GC.Collect();
            WriteLine("GC.Collect() completed");
            WriteLine(await service.Get("a"));
            WriteLine(await service.Get("b"));
            WriteLine(await service.Combine("a", "c"));
            #endregion
        }

        public static async Task Caching4()
        {
            #region Part05_Caching4
            var service = CreateServices().GetRequiredService<Service2>();
            var computed = await Computed.Capture(_ => service.Get("a"));
            WriteLine("computed = GetAsync(a) completed");
            WriteLine(await service.Combine("a", "b"));
            GC.Collect();
            WriteLine("GC.Collect() completed");
            WriteLine(await service.Combine("a", "b"));
            #endregion
        }

        #region Part05_Service3
        public class Service3
        {
            [ComputeMethod]
            public virtual async Task<string> Get(string key)
            {
                WriteLine($"{nameof(Get)}({key})");
                return key;
            }

            [ComputeMethod(KeepAliveTime = 0.3)] // KeepAliveTime was added
            public virtual async Task<string> Combine(string key1, string key2)
            {
                WriteLine($"{nameof(Combine)}({key1}, {key2})");
                return await Get(key1) + await Get(key2);
            }
        }
        #endregion

        public static async Task Caching5()
        {
            #region Part05_Caching5
            var service = CreateServices().GetRequiredService<Service3>();
            WriteLine(await service.Combine("a", "b"));
            WriteLine(await service.Get("a"));
            WriteLine(await service.Get("x"));
            GC.Collect();
            WriteLine("GC.Collect()");
            WriteLine(await service.Combine("a", "b"));
            WriteLine(await service.Get("a"));
            WriteLine(await service.Get("x"));
            await Task.Delay(1000);
            GC.Collect();
            WriteLine("Task.Delay(...) and GC.Collect()");
            WriteLine(await service.Combine("a", "b"));
            WriteLine(await service.Get("a"));
            WriteLine(await service.Get("x"));
            #endregion
        }

        #region Part05_Service4
        public class Service4
        {
            [ComputeMethod(KeepAliveTime = 1), Swap(0.1)]
            public virtual async Task<string> Get(string key)
            {
                WriteLine($"{nameof(Get)}({key})");
                return key;
            }
        }

        public class DemoSwapService : SimpleSwapService
        {
            protected override ValueTask Store(string key, string value, CancellationToken cancellationToken)
            {
                WriteLine($"Swap: {key} <- {value}");
                return base.Store(key, value, cancellationToken);
            }

            protected override ValueTask<bool> Renew(string key, CancellationToken cancellationToken)
            {
                WriteLine($"Swap: {key} <- [try renew]");
                return base.Renew(key, cancellationToken);
            }

            protected override async ValueTask<string?> Load(string key, CancellationToken cancellationToken)
            {
                var result = await base.Load(key, cancellationToken);
                WriteLine($"Swap: {key} -> {result}");
                return result;
            }
        }
        #endregion

        public static async Task Caching6()
        {
            #region Part05_Caching6
            var service = CreateServices().GetRequiredService<Service4>();
            WriteLine(await service.Get("a"));
            await Task.Delay(500);
            GC.Collect();
            WriteLine("Task.Delay(500) and GC.Collect()");
            WriteLine(await service.Get("a"));
            await Task.Delay(1500);
            GC.Collect();
            WriteLine("Task.Delay(1500) and GC.Collect()");
            WriteLine(await service.Get("a"));
            #endregion
        }
    }
}
