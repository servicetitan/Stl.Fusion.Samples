using System;
using System.Reflection;
using Stl.Fusion;

namespace Samples.Blazor.Client.Services
{
    public static class UpdateDelayerEx
    {
        public static bool SetUpdateDelay(this IUpdateDelayer? updateDelayer, TimeSpan delay)
        {
            if (!(updateDelayer is UpdateDelayer))
                return false;
            // A hacky way to change update delay on already existing update delayer
            var field = typeof(UpdateDelayer).GetField("<Delay>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            field!.SetValue(updateDelayer, delay);
            updateDelayer.CancelDelays();
            return true;
        }
    }
}
