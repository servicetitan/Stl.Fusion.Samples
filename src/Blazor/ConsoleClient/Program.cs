using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pluralize.NET;
using Samples.Blazor.Abstractions;
using Samples.Blazor.Client;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Client;
using static System.Console;

var services = CreateServiceProvider();
var stateFactory = services.StateFactory();
var chat = services.GetRequiredService<IChatService>();
var seenMessageIds = new ConcurrentDictionary<long, Unit>();
using var timeState = stateFactory.NewLive<ChatPage>(async (s, cancellationToken) => {
    var chatPage = await chat.GetChatTailAsync(10, cancellationToken);
    foreach (var message in chatPage.Messages) {
        if (!seenMessageIds.TryAdd(message.Id, default))
            continue;
        WriteLine($"{chatPage.Users[message.UserId].Name}: {message.Text}");
    }
    return chatPage;
});
WriteLine("LiveState created, waiting for new chat messages.");
WriteLine("Press <Enter> to stop.");
ReadLine();

static IServiceProvider CreateServiceProvider()
{
    var services = new ServiceCollection();
    services.AddLogging(b => {
        b.ClearProviders();
        b.SetMinimumLevel(LogLevel.Warning);
        b.AddConsole();
    });

    var baseUri = new Uri("http://localhost:5005");
    var apiBaseUri = new Uri($"{baseUri}api/");

    var fusion = services.AddFusion();
    var fusionClient = fusion.AddRestEaseClient(
        (c, o) => {
            o.BaseUri = baseUri;
            o.MessageLogLevel = LogLevel.Information;
        }).ConfigureHttpClientFactory(
        (c, name, o) => {
            var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
            var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
            o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
        });
    var fusionAuth = fusion.AddAuthentication().AddRestEaseClient();

    // Default delay for update delayers
    services.AddSingleton(c => new UpdateDelayer.Options() {
        Delay = TimeSpan.FromSeconds(0.1),
    });

    services.AddSingleton<IPluralize, Pluralizer>();

    // This method registers services marked with any of ServiceAttributeBase descendants, including:
    // [Service], [ComputeService], [RestEaseReplicaService], [LiveStateUpdater]
    services.UseAttributeScanner()
        .AddServicesFrom(Assembly.GetExecutingAssembly())
        .WithScope(Scopes.ClientSideOnly).AddServicesFrom(typeof(ITimeClient).Assembly);

    return services.BuildServiceProvider();
}
