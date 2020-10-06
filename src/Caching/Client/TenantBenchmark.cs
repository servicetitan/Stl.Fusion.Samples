using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Samples.Caching.Common;
using Stl.Frozen;
using Stl.OS;
using Stl.Time;

namespace Samples.Caching.Client
{
    public class TenantBenchmark : BenchmarkBase
    {
        private long _updateErrorCount;

        public int TenantCount { get; set; } = 10_000;
        public int InitializeConcurrencyLevel { get; set; } = HardwareInfo.ProcessorCount;
        public double ReadRatio { get; set; } = 0.999;
        public IServiceProvider Services { get; set; }
        public Func<IServiceProvider, ITenantService> TenantServiceResolver { get; set; } =
            c => c.GetRequiredService<ITenantService>();
        public long UpdateErrorCount => Interlocked.Read(ref _updateErrorCount);

        public TenantBenchmark(IServiceProvider services) => Services = services;

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Initializing ({InitializeConcurrencyLevel} threads)...");
            var tenantIds = new ConcurrentQueue<string>(
                Enumerable.Range(0, TenantCount).Select(i => i.ToString()).ToArray());
            var tasks = Enumerable.Range(0, InitializeConcurrencyLevel).Select(async i => {
                var tenants = TenantServiceResolver(Services);
                var clock = Services.GetRequiredService<IMomentClock>();
                while (tenantIds.TryDequeue(out var tenantId)) {
                    var tenant = await tenants.TryGetAsync(tenantId, cancellationToken).ConfigureAwait(false);
                    if (tenant != null)
                        continue;
                    var now = clock.Now.ToDateTime();
                    tenant = new Tenant() {
                        Id = tenantId,
                        CreatedAt = now,
                        ModifiedAt = now,
                        Name = $"Tenant-{tenantId}",
                        Version = 1,
                    };
                    await tenants.AddOrUpdateAsync(tenant, null, cancellationToken).ConfigureAwait(false);
                }
            });
            var dumpTask = Task.Run(async () => {
                while (!tenantIds.IsEmpty) {
                    Console.WriteLine($"  Remaining tenant count: {tenantIds.Count}");
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                }
            }, cancellationToken);
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Console.WriteLine("  Done.");
        }

        public override string FormatParameters()
            => $"{base.FormatParameters()}, {ReadRatio:P} reads";

        protected override async Task RunAsync(TimeSpan duration, CancellationToken cancellationToken)
        {
            Interlocked.Exchange(ref _updateErrorCount, 0);
            await base.RunAsync(duration, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<long> BenchmarkAsync(int threadIndex, TimeSpan duration, CancellationToken cancellationToken)
        {
            var rnd = new Random(threadIndex * 347);
            var stopwatch = Stopwatch;
            var durationTicks = duration.Ticks;
            var tcIndexMask = TimeCheckOperationIndexMask;
            var tenants = TenantServiceResolver(Services);

            async Task<Tenant> ReadAsync()
            {
                var tenantId = rnd.Next(0, TenantCount).ToString();
                return await tenants.GetAsync(tenantId, cancellationToken);
            }

            async Task UpdateAsync()
            {
                var tenant = await ReadAsync().ConfigureAwait(false);
                tenant = tenant.ToUnfrozen();
                tenant.Name = $"Tenant-{tenant.Id}-{tenant.Version + 1}";
                try {
                    await tenants.AddOrUpdateAsync(tenant, tenant.Version, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception) {
                    Interlocked.Increment(ref _updateErrorCount);
                }
            }

            // The benchmarking loop
            var operationIndex = 0L;
            for (; (operationIndex & tcIndexMask) != 0 || stopwatch.ElapsedTicks < durationTicks; operationIndex++) {
                if (rnd.NextDouble() < ReadRatio)
                    await ReadAsync().ConfigureAwait(false);
                else
                    await UpdateAsync().ConfigureAwait(false);
            }
            return operationIndex;
        }
    }
}
