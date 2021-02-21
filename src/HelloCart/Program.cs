using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Samples.HelloCart;
using Samples.HelloCart.V1;
using Stl.Async;
using Stl.Extensibility;
using static System.Console;

// This is our initial data
var pApple = new Product { Id = "apple", Price = 2M };
var pBanana = new Product { Id = "banana", Price = 0.5M };
var pCarrot = new Product { Id = "carrot", Price = 1M };
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

// Creating services & add initial data there
await using var services = new ServiceCollection()
    .UseModules(modules => modules.Add<InMemoryModule>())
    .AddSingleton<Watcher>()
    .BuildServiceProvider();
var products = services.GetRequiredService<IProductService>();
await products.EditAsync(new EditCommand<Product>(pApple));
await products.EditAsync(new EditCommand<Product>(pBanana));
await products.EditAsync(new EditCommand<Product>(pCarrot));
var carts = services.GetRequiredService<ICartService>();
await carts.EditAsync(new EditCommand<Cart>(cart1));
await carts.EditAsync(new EditCommand<Cart>(cart2));

// Starting watch tasks
var watcher = services.GetRequiredService<Watcher>();
using var stopCts = new CancellationTokenSource();
WriteLine("Initial state:");
watcher.WatchAsync(new[] {pApple, pBanana, pCarrot}, new[] {cart1, cart2}, stopCts.Token).Ignore();
await Task.Delay(100); // Just to make sure watch tasks print whatever they want before our prompt appears

WriteLine();
WriteLine("Change product price by typing [productId]=[price], e.g. \"apple=0\".");
WriteLine("See the total of every affected cart changes.");
while (true) {
    await Task.Delay(100);
    WriteLine();
    Write("[productId]=[price]: ");
    try {
        var parts = (ReadLine() ?? "").Split("=");
        if (parts.Length != 2)
            throw new ApplicationException("Invalid price expression.");
        var productId = parts[0];
        var price = decimal.Parse(parts[1]);
        var product = await products.FindAsync(productId);
        if (product == null)
            throw new KeyNotFoundException("Specified product doesn't exist.");
        await products.EditAsync(new EditCommand<Product>(product with { Price = price }));
    }
    catch (Exception e) {
        WriteLine($"Error: {e.Message}");
    }
}
