using System;

namespace Samples.Caching.Common
{
    public abstract record VersionedEntityBase
    {
        public long Version { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime ModifiedAt { get; init; }
    }
}
