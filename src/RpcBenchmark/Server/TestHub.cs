using Microsoft.AspNetCore.SignalR;

namespace Samples.RpcBenchmark.Server;

public class TestHub : Hub
{
    private readonly TestService _service;

    public TestHub(TestService service)
        => _service = service;

    public Task<HelloReply> SayHello(HelloRequest request)
        => _service.SayHello(request);

    public Task<User?> GetUser(long userId)
        => _service.GetUser(userId);
}
