using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Samples.Benchmark.Server;

public static class EndpointRouteBuilderExt
{
    public static IEndpointRouteBuilder MapTestService<TTestService>(
        this IEndpointRouteBuilder endpoints, string prefix)
        where TTestService : ITestService
    {
        var service = (ITestService)endpoints.ServiceProvider.GetRequiredService<TTestService>();
        endpoints.MapPost($"{prefix}/{nameof(service.AddOrUpdate)}",
            ([FromBody] TestItem testEntity, long? version, CancellationToken cancellationToken)
                => service.AddOrUpdate(testEntity, version, cancellationToken));
        endpoints.MapPost($"{prefix}/{nameof(service.Remove)}", service.Remove);
        endpoints.MapGet($"{prefix}/{nameof(service.GetAll)}", service.GetAll);
        endpoints.MapGet($"{prefix}/{nameof(service.TryGet)}", service.TryGet);
        return endpoints;
    }
}
