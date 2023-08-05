using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Templates.TodoApp.UI;

public class Program
{
    public static Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        StartupHelper.ConfigureServices(builder.Services, builder);
        var host = builder.Build();
        return host.RunAsync();
    }
}
