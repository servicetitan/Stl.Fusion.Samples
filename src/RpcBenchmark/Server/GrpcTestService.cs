using Grpc.Core;

namespace Samples.RpcBenchmark.Server;

public class GrpcTestService : GrpcService.GrpcServiceBase
{
    public override Task<GrpcHelloReply> SayHello(GrpcHelloRequest request, ServerCallContext context)
        => Task.FromResult(new GrpcHelloReply() { Response = request.Request });

    public override Task<GrpcGetUserReply> GetUser(GrpcGetUserRequest request, ServerCallContext context)
    {
        var reply = new GrpcGetUserReply();
        if (request.UserId > 0)
            reply.User = Examples.GrpcUser;
        return Task.FromResult(reply);
    }

    public override Task<GrpcSumReply> Sum(GrpcSumRequest request, ServerCallContext context)
        => Task.FromResult(new GrpcSumReply() { Sum = request.A + request.B });
}
