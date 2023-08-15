using MagicOnion;

namespace Samples.RpcBenchmark;

public interface IMagicOnionTestService : IService<IMagicOnionTestService>
{
    UnaryResult<HelloReply> SayHello(HelloRequest request);
    UnaryResult<User?> GetUser(long userId);
    UnaryResult<int> Sum(int a, int b);
}
