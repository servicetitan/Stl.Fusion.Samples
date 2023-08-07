namespace Samples.Benchmark.Server;

public class FusionTestService : DbTestService, IFusionTestService
{
    public FusionTestService(IServiceProvider services) : base(services) { }

    public override async Task AddOrUpdate(TestItem item, long? version, CancellationToken cancellationToken = default)
    {
        await base.AddOrUpdate(item, version, cancellationToken).ConfigureAwait(false);
        using (Computed.Invalidate()) {
            _ = TryGet(item.Id, default);
            _ = GetAll(default);
        }
    }

    public override async Task Remove(long itemId, long version, CancellationToken cancellationToken = default)
    {
        await base.Remove(itemId, version, cancellationToken).ConfigureAwait(false);
        using (Computed.Invalidate()) {
            _ = TryGet(itemId, default);
            _ = GetAll(default);
        }
    }
}
