using Samples.Benchmark.Server;
using Stl.RestEase;

namespace Samples.Benchmark.Client;

public static class ClientServices
{
    public static readonly IServiceProvider DbServices;
    public static readonly Func<ITestService> LocalDbServiceFactory;
    public static readonly Func<ITestService> LocalFusionServiceFactory;
    public static readonly Func<ITestService> RemoteDbServiceViaHttpFactory;
    public static readonly Func<ITestService> RemoteFusionServiceViaHttpFactory;
    public static readonly Func<ITestService> RemoteFusionServiceViaRpcFactory;
    public static readonly Func<ITestService> RemoteFusionServiceFactory;

    static ClientServices()
    {
        DbServices = new ServiceCollection().AddAppDbContext().BuildServiceProvider();

        // Local factories
        LocalDbServiceFactory = () => DbServices.GetRequiredService<DbTestService>();
        {
            var services = CreateBaseServiceCollection();
            services.AddAppDbContext();
            services.AddFusion().AddService<FusionTestService>();
            var c = services.BuildServiceProvider();
            LocalFusionServiceFactory = () => c.GetRequiredService<FusionTestService>();
        }

        // Remote factories
        {
            var services = CreateBaseServiceCollection();
            services.AddFusion().AddClient<IFusionTestService>();
            RemoteFusionServiceFactory = () => {
                var c = services.BuildServiceProvider();
                return c.GetRequiredService<IFusionTestService>();
            };
        }
        {
            var services = CreateBaseServiceCollection();
            services.AddFusion().Rpc.AddClient<IRpcTestService>();
            RemoteFusionServiceViaRpcFactory = () => {
                var c = services.BuildServiceProvider();
                return c.GetRequiredService<IRpcTestService>();
            };
        }
        {
            var services = CreateBaseServiceCollection();
            services.AddRestEase().AddClient<IFusionTestServiceClientDef>();
            services.AddSingleton<HttpFusionTestServiceClient>();
            RemoteFusionServiceViaHttpFactory = () => {
                var c = services.BuildServiceProvider();
                return c.GetRequiredService<HttpFusionTestServiceClient>();
            };
        }
        {
            var services = CreateBaseServiceCollection();
            services.AddRestEase().AddClient<IDbTestServiceClientDef>();
            services.AddSingleton<HttpDbTestServiceClient>();
            RemoteDbServiceViaHttpFactory = () => {
                var c = services.BuildServiceProvider();
                return c.GetRequiredService<HttpDbTestServiceClient>();
            };
        }
    }

    // Private methods

    private static IServiceCollection CreateBaseServiceCollection()
    {
        var services = new ServiceCollection();
        var fusion = services.AddFusion();
        fusion.Rpc.AddWebSocketClient(BaseUrl);

        var restEase = services.AddRestEase();
        var baseAddress = new Uri(BaseUrl);
        restEase.ConfigureHttpClient((_, name, o) => {
            o.HttpClientActions.Add(c => c.BaseAddress = baseAddress);
        });
        return services;
    }
}
