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
using Stl.Fusion.Extensions;
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
        }).ConfigureHttpClientFactory(
        (c, name, o) => {
            var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
            var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
            o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
        });
    var fusionAuth = fusion.AddAuthentication().AddRestEaseClient();

    // Default update delay is set to 0.1s
    services.AddSingleton<IUpdateDelayer>(_ => new UpdateDelayer(0.1));

    // This method registers services marked with any of ServiceAttributeBase descendants, including:
    // [Service], [ComputeService], [RestEaseReplicaService], [LiveStateUpdater]
    //**
    // services.UseAttributeScanner()
        // .WithScope(Scopes.ClientSideOnly).AddServicesFrom(typeof(ITimeClient).Assembly);
    //**
    fusion.AddFusionTime();
    fusionClient.AddReplicaService<ITimeService, ITimeClient>();
    fusionClient.AddReplicaService<IScreenshotService, IScreenshotClient>();
    fusionClient.AddReplicaService<IChatService, IChatClient>();
    fusionClient.AddReplicaService<IComposerService, IComposerClient>();
    fusionClient.AddReplicaService<ISumService, ISumClient>();

    return services.BuildServiceProvider();
}
