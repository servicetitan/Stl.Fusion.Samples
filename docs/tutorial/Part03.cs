using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stl.DependencyInjection;
using Stl.Fusion;
using static System.Console;

namespace Tutorial
{
    public static class Part03
    {
        #region Part03_CounterService
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

        public static async Task MutableState()
        {
            #region Part03_MutableState
            var services = CreateServices();
            var stateFactory = services.GetStateFactory();
            var state = stateFactory.NewMutable<int>(1);
            var computed = state.Computed;
            WriteLine($"Value: {state.Value}, Computed: {state.Computed}");
            state.Value = 2;
            WriteLine($"Value: {state.Value}, Computed: {state.Computed}");
            WriteLine($"Old computed: {computed}"); // Should be invalidated
            #endregion
        }

        public static async Task MutableStateError()
        {
            #region Part03_MutableStateError
            var services = CreateServices();
            var stateFactory = services.GetStateFactory();
            var state = stateFactory.NewMutable<int>();
            var computed = state.Computed;
            WriteLine($"Value: {state.Value}, Computed: {state.Computed}");
            WriteLine("Setting state.Error.");
            state.Error = new ApplicationException("Just a test");
            try {
                WriteLine($"Value: {state.Value}, Computed: {state.Computed}");
            }
            catch (ApplicationException) {
                WriteLine($"Error: {state.Error.GetType()}, Computed: {state.Computed}");
            }
            WriteLine($"LastValue: {state.LastValue}, LastValueComputed: {state.LastValueComputed}");
            #endregion
        }

        public static async Task LiveState()
        {
            #region Part03_LiveState
            var services = CreateServices();
            var counters = services.GetService<CounterService>();
            var stateFactory = services.GetStateFactory();
            WriteLine("Creating aCounterState.");
            using var aCounterState = stateFactory.NewLive<string>(
                options => {
                    options.WithUpdateDelayer(TimeSpan.FromSeconds(1)); // 1 second update delay
                    options.Invalidated += state => WriteLine($"{DateTime.Now}: Invalidated, Computed: {state.Computed}");
                    options.Updated     += state => WriteLine($"{DateTime.Now}: Updated, Value: {state.Value}, Computed: {state.Computed}");
                },
                async (state, cancellationToken) => {
                    var counter = await counters.GetAsync("a");
                    return $"counters.GetAsync(a) -> {counter}";
                });
            WriteLine("Before aCounterState.UpdateAsync(false).");
            await aCounterState.UpdateAsync(false); // Ensures the state gets up-to-date value
            WriteLine("After aCounterState.UpdateAsync(false).");
            counters.Increment("a");
            await Task.Delay(2000);
            WriteLine($"Value: {aCounterState.Value}, Computed: {aCounterState.Computed}");
            #endregion
        }
    }
}
