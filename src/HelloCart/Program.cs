using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Samples.HelloCart;
using Samples.HelloCart.V1;
using Samples.HelloCart.V2;
using Stl.Async;
using static System.Console;

// Create services
// await using var app = new InMemoryApp();
await using var app = new DbApp();

// Add initial data there
await app.InitializeAsync();

// Starting watch tasks
WriteLine("Initial state:");
using var cts = new CancellationTokenSource();
var watchTask = Task.Run(() => app.WatchAsync(cts.Token));
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
        var productId = parts[0].Trim();
        var price = decimal.Parse(parts[1].Trim());
        var product = await app.ClientProductService.FindAsync(productId);
        if (product == null)
            throw new KeyNotFoundException("Specified product doesn't exist.");
        await app.ClientProductService.EditAsync(new EditCommand<Product>(product with { Price = price }));
    }
    catch (Exception e) {
        WriteLine($"Error: {e.Message}");
    }
}
