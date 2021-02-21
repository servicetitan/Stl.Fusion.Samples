using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;
using static System.Console;

namespace Samples.HelloCart
{
    public class Watcher
    {
        private readonly IProductService _products;
        private readonly ICartService _carts;

        public Watcher(IProductService products, ICartService carts)
        {
            _products = products;
            _carts = carts;
        }

        public Task WatchAsync(Product[] products, Cart[] carts, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            foreach (var product in products)
                tasks.Add(WatchProductAsync(product.Id, cancellationToken));
            foreach (var cart in carts)
                tasks.Add(WatchCartTotalAsync(cart.Id, cancellationToken));
            return Task.WhenAll(tasks);
        }

        public async Task WatchProductAsync(string productId, CancellationToken cancellationToken)
        {
            var computed = await Computed.CaptureAsync(ct => _products.FindAsync(productId, ct), cancellationToken);
            while (!cancellationToken.IsCancellationRequested) {
                WriteLine($"  {computed.Value}");
                await computed.WhenInvalidatedAsync(cancellationToken);
                // Computed instances are ~ immutable, so update means getting a new one
                computed = await computed.UpdateAsync(false, cancellationToken);
            }
        }

        public async Task WatchCartTotalAsync(string cartId, CancellationToken cancellationToken)
        {
            var computed = await Computed.CaptureAsync(ct => _carts.GetTotalAsync(cartId, ct), cancellationToken);
            while (!cancellationToken.IsCancellationRequested) {
                WriteLine($"  {cartId}: total = {computed.Value}");
                await computed.WhenInvalidatedAsync(cancellationToken);
                // Computed instances are ~ immutable, so update means getting a new one
                computed = await computed.UpdateAsync(false, cancellationToken);
            }
        }
    }
}
