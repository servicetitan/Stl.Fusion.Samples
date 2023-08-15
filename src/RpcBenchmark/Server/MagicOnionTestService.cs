using MagicOnion;
using MagicOnion.Server;

namespace Samples.RpcBenchmark.Server;

public class MagicOnionTestService : ServiceBase<IMagicOnionTestService>, IMagicOnionTestService
{
    public UnaryResult<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
        => new(new HelloReply() { Response = request.Request });

    public UnaryResult<User?> GetUser(long userId, CancellationToken cancellationToken = default)
        => new(userId > 0 ? Examples.User : null);

    public UnaryResult<int> Sum(int a, int b, CancellationToken cancellationToken = default)
        => new(a + b);
}
