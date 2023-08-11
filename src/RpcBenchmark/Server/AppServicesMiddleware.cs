using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Samples.RpcBenchmark.Server;

public class AppServicesMiddleware(RequestDelegate next, IServiceProvider services)
{
    private readonly AppServicesFeature _appServicesFeature = new(services);

    public Task InvokeAsync(HttpContext context)
    {
        // Configure request to use application services to avoid creating a request scope
        context.Features.Set<IServiceProvidersFeature>(_appServicesFeature);
        return next.Invoke(context);
    }

    private class AppServicesFeature(IServiceProvider requestServices) : IServiceProvidersFeature
    {
        public IServiceProvider RequestServices { get; set; } = requestServices;
    }
}
