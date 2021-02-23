using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Samples.HelloCart;
using Samples.HelloCart.V1;
using Samples.HelloCart.V2;
using Samples.HelloCart.V3;
using Stl.Async;
using static System.Console;

// Create services
AppBase? app;
while(true) {
    WriteLine("Select the implementation to use:");
    WriteLine("  1: ConcurrentDictionary-based");
    WriteLine("  2: EF Core + Operations Framework (OF)");
    WriteLine("  3: EF Core + DbEntityResolvers + OF");
    // WriteLine("  4: 3 + client-server mode");
    Write("Type 1..3: ");
    app = (ReadLine() ?? "").Trim() switch {
        "1" => new AppV1(),
        "2" => new AppV2(),
        "3" => new AppV3(),
        _ => null,
    };
    if (app != null)
        break;
    WriteLine("Invalid selection.");
    WriteLine();
}
await using var appDisposable = app;
await app.InitializeAsync();

// Starting watch tasks
WriteLine("Initial state:");
using var cts = new CancellationTokenSource();
app.WatchAsync(cts.Token).Ignore();
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
