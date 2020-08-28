using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stl;
using Stl.Fusion;
using static System.Console;

namespace Tutorial
{
    public static class Part01
    {
        public static async Task Create()
        {
            #region part01_create
            // Later we'll show you much nicer ways to create IComputed instances,
            // but for now let's stick to the basics:
            var stateFactory = new ServiceCollection()
                .AddFusionCore()
                .BuildServiceProvider()
                .GetStateFactory();
            var c = stateFactory.NewComputed<int>(async (state, ct) => state.Value + 1).Computed;
            WriteLine($"{c}, Value = {c.Value}");
            WriteLine($"Properties:");
            WriteLine($"{nameof(c.Value)}: {c.Value}");
            WriteLine($"{nameof(c.Error)}: {c.Error}");
            WriteLine($"{nameof(c.Output)}: {c.Output}");
            WriteLine($"{nameof(c.ConsistencyState)}: {c.ConsistencyState}");
            WriteLine($"{nameof(c.Version)}: {c.Version}"); // Similar to ETag in HTTP
            #endregion
        }

        public static async Task InvalidateAndUpdate()
        {
            #region part01_invalidateAndUpdate
            var stateFactory = new ServiceCollection()
                .AddFusionCore()
                .BuildServiceProvider()
                .GetStateFactory();
            var c = stateFactory.NewComputed<int>(async (state, ct) => state.Value + 1).Computed;
            c.Invalidate();
            WriteLine($"{c}, Value = {c.Value}"); // Must be in Invalidated state

            var c1 = await c.UpdateAsync(false);
            WriteLine($"{c1}, Value = {c1.Value}"); // Must be in Consistent state

            // Equality isn't overriden for any implementation of IComputed,
            // so it relies on default Equals & GetHashCode (by-ref comparison).
            WriteLine($"Are {nameof(c)} and {nameof(c1)} pointing to the same instance? {c == c1}");
            #endregion
        }

        public static async Task CreateNoDefault()
        {
            #region part01_createNoDefault
            var stateFactory = new ServiceCollection()
                .AddFusionCore()
                .BuildServiceProvider()
                .GetStateFactory();
            var c = stateFactory.NewComputed<DateTime>(async (state, ct) => DateTime.Now).Computed;
            WriteLine($"{c}, Value = {c.Value}"); // Must be in Invalidated state
            c = await c.UpdateAsync(false);
            WriteLine($"{c}, Value = {c.Value}"); // Must be in Consistent state
            #endregion
        }
    }
}
