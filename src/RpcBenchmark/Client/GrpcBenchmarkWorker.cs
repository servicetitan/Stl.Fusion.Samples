using Grpc.Core;
using CallOptions = Grpc.Core.CallOptions;

namespace Samples.RpcBenchmark.Client;

public sealed class GrpcBenchmarkWorker(ITestService client) : BenchmarkWorker(client)
{
    public readonly GrpcService.GrpcServiceClient GrpcClient = ((GrpcTestClient)client).Client;

    public override async Task SayHello(CancellationToken cancellationToken)
    {
        var result = await GrpcClient
            .SayHelloAsync(Examples.GrpcHelloRequest, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (result.Response.Int32 != 32)
            throw new InvalidOperationException("Wrong result.");
    }

    public override async Task GetUser(CancellationToken cancellationToken)
    {
        var reply = await GrpcClient
            .GetUserAsync(new GrpcGetUserRequest() { UserId = 1 }, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (reply.User.Id != 1)
            throw new InvalidOperationException("Wrong result.");
    }

    public override async Task Sum(CancellationToken cancellationToken)
    {
        var result = await GrpcClient
            .SumAsync(new GrpcSumRequest { A = 1, B = 2 }, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (result.Sum != 3)
            throw new InvalidOperationException("Wrong result.");
    }


    public override async Task StreamS(CancellationToken cancellationToken)
    {
        var request = new GrpcGetItemsRequest() {
            DataSize = DataSizeS,
            DelayEvery = DelayEveryS,
            Count = StreamLength,
        };
        var callOptions = new CallOptions(cancellationToken: cancellationToken);
        var stream = GrpcClient.GetItems(request, callOptions);
        var count = await stream.ResponseStream
            .ReadAllAsync(cancellationToken)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
        if (count != StreamLength)
            throw new InvalidOperationException("Wrong result.");
    }

    public override async Task StreamL(CancellationToken cancellationToken)
    {
        var request = new GrpcGetItemsRequest() {
            DataSize = DataSizeL,
            DelayEvery = DelayEveryL,
            Count = StreamLength,
        };
        var callOptions = new CallOptions(cancellationToken: cancellationToken);
        var stream = GrpcClient.GetItems(request, callOptions);
        var count = await stream.ResponseStream
            .ReadAllAsync(cancellationToken)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
        if (count != StreamLength)
            throw new InvalidOperationException("Wrong result.");
    }

}
