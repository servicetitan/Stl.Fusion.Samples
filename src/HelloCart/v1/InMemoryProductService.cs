using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stl.Async;
using Stl.Fusion;

namespace Samples.HelloCart.V1
{
    public class InMemoryProductService : IProductService
    {
        private readonly ConcurrentDictionary<string, Product> _products = new();

        public virtual Task EditAsync(EditCommand<Product> command, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(command.Id))
                throw new ArgumentOutOfRangeException(nameof(command));
            if (Computed.IsInvalidating()) {
                FindAsync(command.Id, default).Ignore();
                return Task.CompletedTask;
            }

            if (command.Value == null)
                _products.Remove(command.Id, out _);
            else
                _products[command.Id] = command.Value;
            return Task.CompletedTask;
        }

        public virtual Task<Product?> FindAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(_products.GetValueOrDefault(id));
    }
}
