using Stl.Interception;

namespace Samples.RpcBenchmark;

public interface ITestJsonRpcService : ITestService, IRequiresFullProxy
{
    IAsyncEnumerable<Item> GetItemsAlt(GetItemsRequest request, CancellationToken cancellationToken = default);
}
