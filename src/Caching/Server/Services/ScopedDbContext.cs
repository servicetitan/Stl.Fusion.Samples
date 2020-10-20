using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stl.Internal;

namespace Samples.Caching.Server.Services
{
    public abstract class ScopedDbContext : DbContext
    {
        private volatile IServiceScope? _scope;

        public IServiceScope? Scope {
            get => _scope;
            set {
                if (Interlocked.CompareExchange(ref _scope, value, null) != null)
                    throw Errors.AlreadyInitialized(nameof(Scope));
            }
        }

        protected ScopedDbContext() { }
        protected ScopedDbContext(DbContextOptions options) : base(options) { }

        public override void Dispose()
        {
            var scope = Interlocked.Exchange(ref _scope, null);
            if (scope != null)
                scope.Dispose();
            else
                base.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            var scope = Interlocked.Exchange(ref _scope, null);
            if (scope != null)
                scope.Dispose();
            else
                await base.DisposeAsync().ConfigureAwait(false);
        }
    }
}
