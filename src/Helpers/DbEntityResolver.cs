using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stl;
using Stl.Async;
using Stl.OS;

namespace Samples.Helpers
{
    public interface IDbEntityResolver<TDbContext, TKey, TEntity>
        where TDbContext : ScopedDbContext
        where TKey : notnull
        where TEntity : class
    {
        Task<TEntity> GetAsync(TKey key, CancellationToken cancellationToken = default);
        Task<Option<TEntity>> TryGetAsync(TKey key, CancellationToken cancellationToken = default);
        Task<Dictionary<TKey, TEntity>> GetManyAsync(IEnumerable<TKey> keys, CancellationToken cancellationToken = default);
    }

    // This type queues (when needed) & batches calls to TryGetAsync with AsyncBatchProcessor
    // to reduce the rate of underlying DB queries.
    public class DbEntityResolver<TDbContext, TKey, TEntity> : DbServiceBase<TDbContext>,
        IDbEntityResolver<TDbContext, TKey, TEntity>, IDisposable
        where TDbContext : ScopedDbContext
        where TKey : notnull
        where TEntity : class
    {
        protected static MethodInfo ContainsMethod { get; } = typeof(HashSet<TKey>).GetMethod(nameof(HashSet<TKey>.Contains))!;

        private readonly Lazy<AsyncBatchProcessor<TKey, Option<TEntity>>> _batchProcessorLazy;
        protected Func<DbEntityResolver<TDbContext, TKey, TEntity>, AsyncBatchProcessor<TKey, Option<TEntity>>> BatchProcessorFactory { get; set; }
        protected AsyncBatchProcessor<TKey, Option<TEntity>> BatchProcessor => _batchProcessorLazy.Value;
        protected Func<Expression, Expression> KeyExtractorExpressionBuilder { get; set; }
        protected Func<TEntity, TKey> KeyExtractor { get; set; }

        public DbEntityResolver(IServiceProvider services) : base(services)
        {
            _batchProcessorLazy = new Lazy<AsyncBatchProcessor<TKey, Option<TEntity>>>(
                () => BatchProcessorFactory.Invoke(this));
            BatchProcessorFactory = self => new AsyncBatchProcessor<TKey, Option<TEntity>> {
                MaxBatchSize = 16,
                ConcurrencyLevel = Math.Min(HardwareInfo.ProcessorCount, 4),
                BatchingDelayTaskFactory = cancellationToken => Task.Delay(1, cancellationToken),
                BatchProcessor = self.ProcessBatchAsync,
            };

            using var dbContext = services.RentDbContext<TDbContext>();
            var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
            var key = entityType.FindPrimaryKey();
            KeyExtractorExpressionBuilder = eEntity => Expression.PropertyOrField(eEntity, key.Properties.Single().Name);

            var pEntity = Expression.Parameter(typeof(TEntity), "e");
            var eBody = KeyExtractorExpressionBuilder.Invoke(pEntity);
            KeyExtractor = (Func<TEntity, TKey>) Expression.Lambda(eBody, pEntity).Compile();
        }

        void IDisposable.Dispose()
        {
            if (_batchProcessorLazy.IsValueCreated)
                BatchProcessor.Dispose();
        }

        public async Task<TEntity> GetAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var entityOpt = await TryGetAsync(key, cancellationToken).ConfigureAwait(false);
            if (entityOpt.IsSome(out var entity))
                return entity;
            throw new KeyNotFoundException();
        }

        public async Task<Option<TEntity>> TryGetAsync(TKey key, CancellationToken cancellationToken = default)
            => await BatchProcessor.ProcessAsync(key, cancellationToken).ConfigureAwait(false);

        public async Task<Dictionary<TKey, TEntity>> GetManyAsync(IEnumerable<TKey> keys, CancellationToken cancellationToken)
        {
            var tasks = keys.Distinct().Select(key => TryGetAsync(key, cancellationToken)).ToArray();
            var entities = await Task.WhenAll(tasks).ConfigureAwait(false);
            var result = new Dictionary<TKey, TEntity>();
            foreach (var entityOpt in entities)
                if (entityOpt.IsSome(out var entity))
                    result.Add(KeyExtractor.Invoke(entity), entity);
            return result;
        }

        // Protected methods

        protected virtual async Task ProcessBatchAsync(List<BatchItem<TKey, Option<TEntity>>> batch, CancellationToken cancellationToken)
        {
            await using var dbContext = RentDbContext();
            var keys = new HashSet<TKey>();
            foreach (var item in batch) {
                if (!item.TryCancel(cancellationToken))
                    keys.Add(item.Input);
            }
            var pEntity = Expression.Parameter(typeof(TEntity), "e");
            var eKey = KeyExtractorExpressionBuilder.Invoke(pEntity);
            var eBody = Expression.Call(Expression.Constant(keys), ContainsMethod, eKey);
            var eLambda = (Expression<Func<TEntity, bool>>) Expression.Lambda(eBody, pEntity);
            var entities = await dbContext.Set<TEntity>()
                .Where(eLambda)
                .ToDictionaryAsync(KeyExtractor, cancellationToken)
                .ConfigureAwait(false);

            foreach (var item in batch) {
                var entityOpt = entities.TryGetValue(item.Input, out var entity)
                    ? Option.Some(entity)
                    : Option<TEntity>.None;
                item.SetResult(Result.Value(entityOpt), default);
            }
        }
    }
}
