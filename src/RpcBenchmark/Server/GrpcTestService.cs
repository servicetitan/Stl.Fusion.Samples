using Google.Protobuf;
using Grpc.Core;

namespace Samples.RpcBenchmark.Server;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

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

    public override async Task GetItems(GrpcGetItemsRequest request, IServerStreamWriter<GrpcItem> responseStream, ServerCallContext context)
    {
        var dataSize = request.DataSize;
        var count = request.Count;
        var cancellationToken = context.CancellationToken;
        if (dataSize < 0)
            throw new ArgumentOutOfRangeException(nameof(dataSize));

        for (var i = 0; i < count; i++) {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.DelayEvery > 0 && (i % request.DelayEvery) == 0)
                await Task.Delay(10, cancellationToken).ConfigureAwait(false);

            var item = new GrpcItem() {
                Index = i,
                Data = ByteString.CopyFrom(new byte[dataSize]),
            };
            await responseStream.WriteAsync(item, cancellationToken).ConfigureAwait(false);
        }
    }
}
