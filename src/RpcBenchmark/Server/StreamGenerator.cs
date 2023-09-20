using System.Runtime.CompilerServices;

namespace Samples.RpcBenchmark.Server;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

public static class StreamGenerator
{
    public static async IAsyncEnumerable<Item> GetItems(
        GetItemsRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (request.DataSize < 0)
            throw new ArgumentOutOfRangeException(nameof(request));

        for (var i = 0; i < request.Count; i++) {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.DelayEvery > 0 && (i % request.DelayEvery) == 0)
                await Task.Delay(10, cancellationToken).ConfigureAwait(false);

            yield return new Item() {
                Index = i,
                Data = new byte[request.DataSize],
            };
        }
    }
}
