using System;
using System.Threading.Tasks;
using Pluralize.NET;
using Stl.Async;
using Stl.Fusion;

namespace Samples.HelloBlazorServer.Services
{
    [ComputeService]
    public class TimeService
    {
        private IPluralize _pluralize;

        public TimeService(IPluralize pluralize) => _pluralize = pluralize;

        [ComputeMethod(AutoInvalidateTime = 0.5)] // Unconditional auto-invalidation
        public virtual Task<DateTime> GetTimeAsync()
            => Task.FromResult(DateTime.Now);

        [ComputeMethod]
        public virtual Task<string> GetMomentsAgoAsync(DateTime time)
        {
            var delta = DateTime.Now - time;
            if (delta < TimeSpan.Zero)
                delta = TimeSpan.Zero;
            var (unit, unitName) = GetMomentsAgoUnit(delta);
            var unitCount = (int) (delta.TotalSeconds / unit.TotalSeconds);
            var pluralizedUnitName = _pluralize.Format(unitName, unitCount);
            var result = $"{unitCount} {pluralizedUnitName} ago";

            // Invalidate the result when it's supposed to change
            var delay = (unitCount + 1) * unit - delta;
            var computed = Computed.GetCurrent();
            Task.Delay(delay, default).ContinueWith(_ => computed.Invalidate()).Ignore();

            return Task.FromResult(result);
        }

        private (TimeSpan Unit, string UnitName) GetMomentsAgoUnit(TimeSpan delta)
        {
            if (delta.TotalSeconds < 60)
                return (TimeSpan.FromSeconds(1), "second");
            if (delta.TotalMinutes < 60)
                return (TimeSpan.FromMinutes(1), "minute");
            if (delta.TotalHours < 24)
                return (TimeSpan.FromHours(1), "hour");
            if (delta.TotalDays < 7)
                return (TimeSpan.FromDays(1), "day");
            return (TimeSpan.FromDays(7), "week");
        }
    }
}
