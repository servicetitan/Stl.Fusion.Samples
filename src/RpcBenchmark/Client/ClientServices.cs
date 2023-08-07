using Microsoft.AspNetCore.SignalR.Client;
using Stl.RestEase;
using Stl.Rpc;

namespace Samples.RpcBenchmark.Client;

public static class ClientServices
{
    public static readonly Func<ITestService> RpcClientService;
    public static readonly Func<ITestService> SignalRClientService;
    public static readonly Func<ITestService> HttpClientService;

    static ClientServices()
    {
        {
            var services = CreateBaseServiceCollection();
            services.AddRpc().AddClient<ITestService>();
            RpcClientService = () => {
                var c = services.BuildServiceProvider();
                return c.GetRequiredService<ITestService>();
            };
        }
        {
            var services = CreateBaseServiceCollection();
            services.AddSingleton<SignalRTestServiceClient>();
            SignalRClientService = () => {
                var c = services.BuildServiceProvider();
                return c.GetRequiredService<SignalRTestServiceClient>();
            };
        }
        {
            var services = CreateBaseServiceCollection();
            services.AddRestEase().AddClient<ITestServiceClientDef>();
            services.AddSingleton<HttpTestServiceClient>();
            HttpClientService = () => {
                var c = services.BuildServiceProvider();
                return c.GetRequiredService<HttpTestServiceClient>();
            };
        }
    }

    // Private methods

    private static IServiceCollection CreateBaseServiceCollection()
    {
        var services = new ServiceCollection();

        // Rpc
        services.AddRpc().AddWebSocketClient(Settings.BaseUrl);

        // SignalR
        services.AddSingleton(_ => {
            var connection = new HubConnectionBuilder()
                .WithUrl($"{Settings.BaseUrl}hubs/testService")
                .Build();
            return connection;
        });

        // RestEase/HTTP
        var restEase = services.AddRestEase();
        var baseAddress = new Uri(Settings.BaseUrl);
        restEase.ConfigureHttpClient((_, name, o) => {
            o.HttpClientActions.Add(c => c.BaseAddress = baseAddress);
        });

        return services;
    }
}
