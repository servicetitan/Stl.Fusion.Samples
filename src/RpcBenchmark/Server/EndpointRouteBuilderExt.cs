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
        return endpoints;
    }
}
