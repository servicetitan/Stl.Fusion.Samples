using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stl.Async;
using Stl.Fusion;

namespace Samples.HelloCart.V1
{
    public class InMemoryCartService : ICartService
    {
        private readonly ConcurrentDictionary<string, Cart> _carts = new();
        private readonly IProductService _products;

        public InMemoryCartService(IProductService products) => _products = products;

        public virtual Task EditAsync(EditCommand<Cart> command, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(command.Id))
                throw new ArgumentOutOfRangeException(nameof(command));
            if (Computed.IsInvalidating()) {
                FindAsync(command.Id, default).Ignore();
                GetTotalAsync(command.Id, default).Ignore();
                return Task.CompletedTask;
            }

            if (command.Value == null)
                _carts.Remove(command.Id, out _);
            else
                _carts[command.Id] = command.Value;
            return Task.CompletedTask;
        }

        public virtual Task<Cart?> FindAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(_carts.GetValueOrDefault(id));

        public virtual async Task<decimal> GetTotalAsync(string id, CancellationToken cancellationToken = default)
        {
            var cart = await FindAsync(id, cancellationToken);
            if (cart == null)
                return 0;
            var total = 0M;
            foreach (var (productId, quantity) in cart.Items) {
                var product = await _products.FindAsync(productId, cancellationToken);
                total += (product?.Price ?? 0M) * quantity;
            }
            return total;
        }
    }
}
