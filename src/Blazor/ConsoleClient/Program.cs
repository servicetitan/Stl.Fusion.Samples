using Samples.Blazor.Abstractions;
using Samples.Blazor.Client;
using Stl.Fusion.Client;
using Stl.Fusion.Extensions;
using Stl.Fusion.UI;
using static System.Console;

var services = CreateServiceProvider();
var stateFactory = services.StateFactory();
var chat = services.GetRequiredService<IChatService>();
var seenMessageIds = new ConcurrentDictionary<long, Unit>();
using var timeState = stateFactory.NewComputed<ChatPage>(async (s, cancellationToken) => {
    var chatPage = await chat.GetChatTail(10, cancellationToken);
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
    services.AddLogging(logging => {
        logging.ClearProviders();
        logging.SetMinimumLevel(LogLevel.Warning);
        logging.AddConsole();
    });

    var baseUri = new Uri("http://localhost:5005");
    var apiBaseUri = new Uri($"{baseUri}api/");

    // Fusion
    var fusion = services.AddFusion();
    var fusionClient = fusion.AddRestEaseClient();
    fusionClient.ConfigureWebSocketChannel(c => new() { BaseUri = baseUri });
    fusionClient.ConfigureHttpClient((c, name, o) => {
        var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
        var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
        o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
    });
    var fusionAuth = fusion.AddAuthentication().AddRestEaseClient();

    // Fusion services
    fusion.AddFusionTime();
    fusionClient.AddReplicaService<ITimeService, ITimeClientDef>();
    fusionClient.AddReplicaService<IScreenshotService, IScreenshotClientDef>();
    fusionClient.AddReplicaService<IChatService, IChatClientDef>();
    fusionClient.AddReplicaService<IComposerService, IComposerClientDef>();
    fusionClient.AddReplicaService<ISumService, ISumClientDef>();

    // Default update delay is 0.1s
    services.AddTransient<IUpdateDelayer>(c => new UpdateDelayer(c.UICommandTracker(), 0.1));

    return services.BuildServiceProvider();
}
