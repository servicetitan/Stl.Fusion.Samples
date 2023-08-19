using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Samples.RpcBenchmark.Server;

public static class EndpointRouteBuilderExt
{
    public static IEndpointRouteBuilder MapTestService<TTestService>(
        this IEndpointRouteBuilder endpoints, string prefix)
        where TTestService : ITestService
    {
        var service = (ITestService)endpoints.ServiceProvider.GetRequiredService<TTestService>();
        endpoints.MapPost($"{prefix}/{nameof(service.SayHello)}",
            (HelloRequest request, CancellationToken cancellationToken)
                => service.SayHello(request, cancellationToken));
        endpoints.MapGet($"{prefix}/{nameof(service.GetUser)}", service.GetUser);
        endpoints.MapGet($"{prefix}/{nameof(service.Sum)}", service.Sum);
        return endpoints;
    }

    public static IEndpointRouteBuilder MapStreamJsonRpcService<TService>(
        this IEndpointRouteBuilder endpoints, string pattern)
        where TService : class, ITestService
    {
        var service = endpoints.ServiceProvider.GetRequiredService<TService>();
        endpoints.MapGet(pattern, context => StreamJsonRpcEndpoint.Invoke(service, context));
        return endpoints;
    }
}
