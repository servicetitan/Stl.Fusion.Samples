using MagicOnion;

namespace Samples.RpcBenchmark;

public interface IMagicOnionTestService : IService<IMagicOnionTestService>
{
    UnaryResult<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default);
    UnaryResult<User?> GetUser(long userId, CancellationToken cancellationToken = default);
    UnaryResult<int> Sum(int a, int b, CancellationToken cancellationToken = default);
}
