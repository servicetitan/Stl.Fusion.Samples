using Stl.Rpc;

namespace Samples.RpcBenchmark;

public interface ITestService : IRpcService
{
    Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default);
    Task<User?> GetUser(long userId, CancellationToken cancellationToken = default);
}
