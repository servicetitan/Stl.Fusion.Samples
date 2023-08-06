using Samples.Benchmark.Server;
using Stl.RestEase;

namespace Samples.Benchmark.Client;

public static class ClientServices
{
    public static readonly IServiceProvider DbServices;
    public static readonly Func<ITenants> LocalDbTenantsFactory;
    public static readonly Func<ITenants> LocalFusionTenantsFactory;
    public static readonly Func<ITenants> HttpClientToDbTenantsFactory;
    public static readonly Func<ITenants> HttpClientToFusionTenantsFactory;
    public static readonly Func<ITenants> RpcClientToFusionTenantsFactory;
    public static readonly Func<ITenants> FusionClientToFusionTenantsFactory;

    static ClientServices()
    {
        DbServices = new ServiceCollection().AddAppDbContext().BuildServiceProvider();

        // Local factories
        LocalDbTenantsFactory = () => DbServices.GetRequiredService<DbTenants>();
        {
            var services = CreateBaseServiceCollection();
            services.AddAppDbContext();
            services.AddFusion().AddService<FusionTenants>();
            var c = services.BuildServiceProvider();
            LocalFusionTenantsFactory = () => c.GetRequiredService<FusionTenants>();
        }

        // Remote factories
        {
            var services = CreateBaseServiceCollection();
            var restEase = services.AddRestEase();
            restEase.AddClient<IDbTenantsClientDef>();
            services.AddTransient(c => new HttpClientToDbTenants(c));
            HttpClientToDbTenantsFactory = () => {
                var c = services.BuildServiceProvider();
                return c.GetRequiredService<HttpClientToDbTenants>();
            };
        }
        {
            var services = CreateBaseServiceCollection();
            var restEase = services.AddRestEase();
            restEase.AddClient<IFusionTenantsClientDef>();
            services.AddTransient(c => new HttpClientToFusionTenants(c));
            HttpClientToFusionTenantsFactory = () => {
                var c = services.BuildServiceProvider();
                return c.GetRequiredService<HttpClientToFusionTenants>();
            };
        }
        {
            var services = CreateBaseServiceCollection();
            services.AddFusion().Rpc.AddClient<IRpcTenants>();
            RpcClientToFusionTenantsFactory = () => {
                var c = services.BuildServiceProvider();
                return c.GetRequiredService<IRpcTenants>();
            };
        }
        {
            var services = CreateBaseServiceCollection();
            services.AddFusion().AddClient<IFusionTenants>();
            FusionClientToFusionTenantsFactory = () => {
                var c = services.BuildServiceProvider();
                return c.GetRequiredService<IFusionTenants>();
            };
        }
    }

    // Private methods

    private static IServiceCollection CreateBaseServiceCollection()
    {
        var services = new ServiceCollection();
        var fusion = services.AddFusion();
        fusion.Rpc.AddWebSocketClient(Settings.BaseUrl);

        var restEase = services.AddRestEase();
        restEase.ConfigureHttpClient((_, name, o) => {
            o.HttpClientActions.Add(c => c.BaseAddress = new Uri(Settings.BaseUrl));
        });

        return services;
    }
}
