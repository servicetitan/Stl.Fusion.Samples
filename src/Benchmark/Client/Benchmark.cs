using Stl.OS;
using static System.Console;

namespace Samples.Benchmark.Client;

public class Benchmark : BenchmarkBase
{
    public int ItemCount { get; init; } = Settings.DbItemCount;
    public int InitConcurrencyLevel { get; init; } = HardwareInfo.ProcessorCount;
    public int WriteConcurrencyLevel { get; init; } = 1;
    public Func<ITenants> TenantsFactory { get; set; } = ClientServices.LocalDbTenantsFactory;

    public async Task Initialize(CancellationToken cancellationToken = default)
    {
        WriteLine($"Initializing using {InitConcurrencyLevel} workers...");
        var tenantIds = new ConcurrentQueue<string>(
            Enumerable.Range(0, ItemCount).Select(i => i.ToString()).ToArray());
        var tasks = Enumerable.Range(0, InitConcurrencyLevel).Select(async i => {
            var tenants = TenantsFactory.Invoke();
            var clock = SystemClock.Instance;
            while (tenantIds.TryDequeue(out var tenantId)) {
                var tenant = await tenants.TryGet(tenantId, cancellationToken).ConfigureAwait(false);
                if (tenant != null)
                    continue;

                var now = clock.Now.ToDateTime();
                tenant = new Tenant() {
                    Id = tenantId,
                    Version = 1,
                    CreatedAt = now,
                    ModifiedAt = now,
                    Name = $"Tenant-{tenantId}",
                };
                await tenants.AddOrUpdate(tenant, null, cancellationToken).ConfigureAwait(false);
            }
        });
        _ = Task.Run(async () => {
            while (!tenantIds.IsEmpty) {
                WriteLine($"  Remaining tenant count: {tenantIds.Count}");
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
            }
        }, cancellationToken);
        await Task.WhenAll(tasks).ConfigureAwait(false);
        WriteLine("  Done.");
    }

    public override void DumpParameters()
    {
        base.DumpParameters();
        WriteLine($"  - {"Writer #",-12}: {WriteConcurrencyLevel}");
        WriteLine($"  - {"Reader #",-12}: {ConcurrencyLevel - WriteConcurrencyLevel}");
    }

    protected override async Task<Dictionary<string, Counter>> RunWorker(int workerId, TimeSpan duration, CancellationToken cancellationToken)
    {
        var rnd = new Random(workerId * 347);
        var stopwatch = Stopwatch;
        var tcIndexMask = TimeCheckOperationIndexMask;
        var tenants = TenantsFactory.Invoke();
        var isWriter = workerId < WriteConcurrencyLevel;

        async Task Read()
        {
            var tenantId = rnd.Next(0, ItemCount).ToString();
            await tenants.Get(tenantId, cancellationToken);
        }

        async Task Write()
        {
            var tenantId = rnd.Next(0, ItemCount).ToString();
            var tenant = await tenants.Get(tenantId, cancellationToken);
            tenant = tenant with { Name = $"Tenant-{tenant.Id}-{tenant.Version + 1}" };
            await tenants.AddOrUpdate(tenant, tenant.Version, cancellationToken).ConfigureAwait(false);
        }

        var operation = isWriter ? (Func<Task>) Write : Read;

        // The benchmarking loop
        var count = 0L;
        var errorCount = 0L;
        for (; (count & tcIndexMask) != 0 || stopwatch.Elapsed < duration; count++) {
            try {
                await operation.Invoke().ConfigureAwait(false);
            }
            catch (Exception) {
                errorCount++;
            }
        }
        return new Dictionary<string, Counter>() {
            { isWriter ? "Writes" : "Reads", new OpsCounter(count) },
            { isWriter ? "Write Errors" : "Read Errors", new OpsCounter(errorCount) }
        };
    }
}
