using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Stl.Collections;

namespace Samples.BoardGames.Abstractions
{
    public static class EnumerableEx
    {
        public static IAsyncEnumerable<TResult> ParallelSelectAsync<T, TResult>(
            this IEnumerable<T> source,
            Func<T, CancellationToken, Task<TResult>> selector,
            CancellationToken cancellationToken = default)
            => source.ParallelSelectAsync(selector, 128, cancellationToken);

        public static async IAsyncEnumerable<TResult> ParallelSelectAsync<T, TResult>(
            this IEnumerable<T> source,
            Func<T, CancellationToken, Task<TResult>> selector,
            int packSize,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var pack in source.PackBy(packSize)) {
                var tasks = pack.Select(i => selector.Invoke(i, cancellationToken));
                var results = await Task.WhenAll(tasks);
                foreach (var result in results)
                    yield return result;
            }
        }

        public static Task<List<TResult>> ParallelSelectToListAsync<T, TResult>(
            this IEnumerable<T> source,
            Func<T, CancellationToken, Task<TResult>> selector,
            CancellationToken cancellationToken = default)
            => source.ParallelSelectToListAsync(selector, 128, cancellationToken);

        public static async Task<List<TResult>> ParallelSelectToListAsync<T, TResult>(
            this IEnumerable<T> source,
            Func<T, CancellationToken, Task<TResult>> selector,
            int packSize,
            CancellationToken cancellationToken = default)
        {
            var result = new List<TResult>();
            foreach (var pack in source.PackBy(packSize)) {
                var tasks = pack.Select(i => selector.Invoke(i, cancellationToken));
                var results = await Task.WhenAll(tasks);
                result.AddRange(results);
            }
            return result;
        }
    }
}
