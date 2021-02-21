using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stl.Async;
using Stl.Fusion;
using static System.Console;

namespace Samples.HelloCart
{
    public abstract class AppBase
    {
        public IServiceProvider Services { get; protected init; } = null!;
        public IProductService ProductService => ClientServices.GetRequiredService<IProductService>();
        public ICartService CartService => ClientServices.GetRequiredService<ICartService>();

        public IServiceProvider ClientServices { get; protected init; } = null!;
        public IProductService ClientProductService => ClientServices.GetRequiredService<IProductService>();
        public ICartService ClientCartService => ClientServices.GetRequiredService<ICartService>();

        public Product[] ExistingProducts { get; set; } = Array.Empty<Product>();
        public Cart[] ExistingCarts { get; set; } = Array.Empty<Cart>();

        public virtual async Task InitializeAsync()
        {
            var pApple = new Product { Id = "apple", Price = 2M };
            var pBanana = new Product { Id = "banana", Price = 0.5M };
            var pCarrot = new Product { Id = "carrot", Price = 1M };
            ExistingProducts = new [] { pApple, pBanana, pCarrot };
            foreach (var product in ExistingProducts)
                await ProductService.EditAsync(new EditCommand<Product>(product));

            var cart1 = new Cart() { Id = "cart:apple=1,banana=2",
                Items = ImmutableDictionary<string, decimal>.Empty
                    .Add(pApple.Id, 1)
                    .Add(pBanana.Id, 2)
            };
            var cart2 = new Cart() { Id = "cart:banana=1,carrot=1",
                Items = ImmutableDictionary<string, decimal>.Empty
                    .Add(pBanana.Id, 1)
                    .Add(pCarrot.Id, 1)
            };
            ExistingCarts = new [] { cart1, cart2 };
            foreach (var cart in ExistingCarts)
                await CartService.EditAsync(new EditCommand<Cart>(cart));
        }

        public async ValueTask DisposeAsync()
        {
            if (ClientServices is IAsyncDisposable csd)
                await csd.DisposeAsync();
            if (Services is IAsyncDisposable sd)
                await sd.DisposeAsync();
        }

        public Task WatchAsync(CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>();
            foreach (var product in ExistingProducts)
                tasks.Add(WatchProductAsync(product.Id, cancellationToken));
            foreach (var cart in ExistingCarts)
                tasks.Add(WatchCartTotalAsync(cart.Id, cancellationToken));
            return Task.WhenAll(tasks);
        }

        public async Task WatchProductAsync(string productId, CancellationToken cancellationToken = default)
        {
            var computed = await Computed.CaptureAsync(ct => ClientProductService.FindAsync(productId, ct), cancellationToken);
            while (true) {
                WriteLine($"  {computed.Value}");
                await computed.WhenInvalidatedAsync(cancellationToken);
                computed = await computed.UpdateAsync(false, cancellationToken);
            }
        }

        public async Task WatchCartTotalAsync(string cartId, CancellationToken cancellationToken = default)
        {
            var computed = await Computed.CaptureAsync(ct => ClientCartService.GetTotalAsync(cartId, ct), cancellationToken);
            while (true) {
                WriteLine($"  {cartId}: total = {computed.Value}");
                await computed.WhenInvalidatedAsync(cancellationToken);
                computed = await computed.UpdateAsync(false, cancellationToken);
            }
        }
    }
}
