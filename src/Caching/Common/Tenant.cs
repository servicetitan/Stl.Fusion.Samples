using System.ComponentModel.DataAnnotations;

namespace Samples.Caching.Common;

public record Tenant : VersionedEntityBase, IHasId<string>
{
    [Key]
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
}
