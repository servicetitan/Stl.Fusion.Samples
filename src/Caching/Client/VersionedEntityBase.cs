using System;
using Stl.Frozen;

namespace Samples.Caching.Client
{
    public abstract class VersionedEntityBase : FrozenBase
    {
        private long _version;
        private DateTime _createdAt;
        private DateTime _modifiedAt;

        public long Version {
            get => _version;
            set { ThrowIfFrozen(); _version = value; }
        }

        public DateTime CreatedAt {
            get => _createdAt;
            set { ThrowIfFrozen(); _createdAt = value; }
        }

        public DateTime ModifiedAt {
            get => _modifiedAt;
            set { ThrowIfFrozen(); _modifiedAt = value; }
        }
    }
}
