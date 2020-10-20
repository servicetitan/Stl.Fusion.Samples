using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Samples.Caching.Common;
using Stl.DependencyInjection;
using Stl.Frozen;
using Stl.OS;
using Stl.Time;
using static System.Console;

namespace Samples.Caching.Client
{
    [Service]
    public class TenantBenchmark : BenchmarkBase
    {
        public int InitConcurrencyLevel { get; set; } = HardwareInfo.ProcessorCount;
        public int WriteConcurrencyLevel { get; set; } = 1;
        public int TenantCount { get; set; } = 10_000;
        public IServiceProvider Services { get; set; }
        public Func<IServiceProvider, ITenantService> TenantServiceResolver { get; set; } =
            c => c.GetRequiredService<ITenantService>();

        public TenantBenchmark(IServiceProvider services) => Services = services;

        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            WriteLine($"Initializing using {InitConcurrencyLevel} workers...");
            var tenantIds = new ConcurrentQueue<string>(
                Enumerable.Range(0, TenantCount).Select(i => i.ToString()).ToArray());
            var tasks = Enumerable.Range(0, InitConcurrencyLevel).Select(async i => {
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

        protected override async Task<Dictionary<string, Counter>> BenchmarkAsync(int workerId, TimeSpan duration, CancellationToken cancellationToken)
        {
            var rnd = new Random(workerId * 347);
            var stopwatch = Stopwatch;
            var tcIndexMask = TimeCheckOperationIndexMask;
            var tenants = TenantServiceResolver(Services);
            var isWriter = workerId < WriteConcurrencyLevel;

            async Task ReadAsync()
            {
                var tenantId = rnd.Next(0, TenantCount).ToString();
                await tenants.GetAsync(tenantId, cancellationToken);
            }

            async Task WriteAsync()
            {
                var tenantId = rnd.Next(0, TenantCount).ToString();
                var tenant = await tenants.GetAsync(tenantId, cancellationToken);
                tenant = tenant.ToUnfrozen();
                tenant.Name = $"Tenant-{tenant.Id}-{tenant.Version + 1}";
                await tenants.AddOrUpdateAsync(tenant, tenant.Version, cancellationToken).ConfigureAwait(false);
            }

            var operationAsync = isWriter ? (Func<Task>) WriteAsync : ReadAsync;

            // The benchmarking loop
            var count = 0L;
            var errorCount = 0L;
            for (; (count & tcIndexMask) != 0 || stopwatch.Elapsed < duration; count++) {
                try {
                    await operationAsync().ConfigureAwait(false);
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
}
