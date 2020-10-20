using System;
using System.Collections.Generic;

namespace Samples.Caching.Client
{
    public abstract class Counter
    {
        public abstract bool HasValue { get; }
        public abstract Counter CombineWith(Counter other);
        public abstract string Format(TimeSpan duration);
    }

    public abstract class Counter<T> : Counter
    {
        public T Value { get; }
        public override bool HasValue => !EqualityComparer<T>.Default.Equals(Value, default);

        protected Counter(T value) => Value = value;

        public override string Format(TimeSpan duration)
            => Value == null ? "n/a" : $"{Value} in {duration.TotalSeconds}s";
    }

    public class OpsCounter : Counter<long>
    {
        public OpsCounter(long value) : base(value) { }

        public override Counter CombineWith(Counter other) => new OpsCounter(Value + ((OpsCounter) other).Value);

        public override string Format(TimeSpan duration)
        {
            var scale = "";
            var value = Value / duration.TotalSeconds;
            if (value >= 1000_000) {
                scale = "M";
                value /= 1000_000;
            }
            else if (value >= 1000) {
                scale = "K";
                value /= 1000;
            }
            return $"{value:N}{scale} operations/s";
        }
    }
}
