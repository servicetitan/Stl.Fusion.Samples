using System.ComponentModel.DataAnnotations;
using Stl;

namespace Samples.Caching.Common
{
    public class Tenant : VersionedEntityBase, IHasId<string>
    {
        private string _id = "";
        private string _name = "";

        [Key]
        public string Id {
            get => _id;
            set { ThrowIfFrozen(); _id = value; }
        }

        public string Name {
            get => _name;
            set { ThrowIfFrozen(); _name = value; }
        }
    }
}
