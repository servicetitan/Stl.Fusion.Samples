using MagicOnion;
using MagicOnion.Server;

namespace Samples.RpcBenchmark.Server;

public class MagicOnionTestService : ServiceBase<IMagicOnionTestService>, IMagicOnionTestService
{
    public UnaryResult<HelloReply> SayHello(HelloRequest request)
        => new(new HelloReply() { Response = request.Request });

    public UnaryResult<User?> GetUser(long userId)
        => new(userId > 0 ? Examples.User : null);

    public UnaryResult<int> Sum(int a, int b)
        => new(a + b);
}
