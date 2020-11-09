using Microsoft.EntityFrameworkCore;

namespace Samples.Helpers
{
    public static class DbContextEx
    {
        public static void DisableChangeTracking(this DbContext dbContext)
        {
            var ct = dbContext.ChangeTracker;
            ct.AutoDetectChangesEnabled = false;
            ct.LazyLoadingEnabled = false;
            ct.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
    }
}
