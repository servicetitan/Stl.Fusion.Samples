![](docs/img/Banner.jpg)

Welcome to a collection of samples for [Stl.Fusion](https://github.com/servicetitan/Stl.Fusion)!

> All project updates are published on our [Discord Server](https://discord.gg/EKEwv6d); it's also the best place for Q/A.\
> [![Build](https://github.com/servicetitan/Stl.Fusion.Samples/workflows/Build/badge.svg)](https://github.com/servicetitan/Stl.Extras/actions?query=workflow%3A%22Build%22)

## What's Inside?

### 1. Hello World Sample

[HelloWorld](src/HelloWorld) is a basic sample showing how to create
simple dependency chains and and react to invalidation events. 
[Its Program.cs](src/HelloWorld/Program.cs) is just about 40 lines long.

![](docs/img/Samples-HelloWorld.gif)

### 2. Hello Blazor Server

[HelloBlazorServer](src/HelloBlazorServer) is a modified version of a
templated Blazor Server App that displays changes made to `CounterService` 
and `WeatherForecastService` in real-time.

The sample shows how to plug Fusion to server-side Blazor app:
1.  `dotnet new blazorserver -o HelloBlazorServer` was used to create Blazor Server App.
2.  [First commit](https://github.com/servicetitan/Stl.Fusion.Samples/commit/334423ab42aa41b5c92dbab61472cda8ef9dab00) 
    adds Fusion packages, makes "Fetch Data" page to update in real time, 
    and finally, turns its `WeatherForecast` to `IComputedService`
    that to auto-invalidates the result of its `GetForecastAsync` every second,
    which effectively makes the page to update every 2 seconds 
    (1 extra second is added by default `IUpdateDelayer`).
3.  [Second commit](https://github.com/servicetitan/Stl.Fusion.Samples/commit/4eed9413a9bb383ef827b0570e1d5bacff6d942c) 
    upgrades "Counter" page to real-time updates, extracts `CounterService`,
    and makes it use a shared counter (the original app uses a local one
    stored right in the page component).

Open this sample in 2 browser windows to see the difference:

![](docs/img/Samples-HelloBlazorServer.gif)

### 3. Blazor Samples

It's a dual-mode [Blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-3.1) SPA hosted by
[ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet) website,
which also serves its API. The application includes:
* "Server Time" and "Server Screen" pages showing the simplest timeout-based invalidation
* "Chat" - a tiny chat relying on event-based invalidation
* "Composition" shows Fusion's ability to use both  local `IComputed<T>` instances 
  and client-side replicas of similar server-side instances to compute a new value
  that properly tracks both local and remote dependencies.

![](docs/img/Samples-Blazor.gif)

The app supports **both (!)** Server-Side Blazor and Blazor WebAssembly modes &ndash;
you can switch the mode on "Home" page.

![](docs/img/Samples-Blazor-DualMode.gif)

Moreover, it also exposes a regular RESTful API &ndash;
try invoking any of endpoints there right from embedded Swagger console.

![](docs/img/SwaggerDoc.jpg)

### 4. Tutorial

It's interactive &ndash; you can simply [browse it](docs/tutorial/README.md), but to
modify and run the C# code presented there, you need
[Try .NET](https://github.com/dotnet/try/blob/master/DotNetTryLocal.md)
or [Docker](https://www.docker.com/).

## Running Samples

Build & run locally with [.NET Core SDK 3.1](https://dotnet.microsoft.com/download):

| Sample | Command |
|-|-|
| [HelloWorld](src/HelloWorld) | `dotnet run -p src/HelloWorld/HelloWorld.csproj` |
| [HelloBlazorServer](src/HelloBlazorServer) |  `dotnet run --project src/HelloBlazorServer/HelloBlazorServer.csproj` + http://localhost:5000/ |
| [Blazor Samples](src/Blazor) |  `dotnet run --project src/Blazor/Server/Server.csproj` + http://localhost:5005/ |
| [Tutorial](docs/tutorial/README.md) | [Install Try .NET (preview version)](https://github.com/dotnet/try/blob/master/DotNetTryLocal.md) + `dotnet try --port 50005 docs/tutorial` |

Build & run with [Docker](https://docs.docker.com/get-docker/) + 
[Docker Compose](https://docs.docker.com/compose/install/):

| Sample | Command |
|-|-|
| [HelloWorld](src/HelloWorld) | `docker-compose run sample_hello_world dotnet Samples.HelloWorld.dll` |
| [HelloBlazorServer](src/HelloBlazorServer) | `docker-compose up --build sample_hello_blazor_server` + http://localhost:5000/ |
| [Blazor Samples](src/Blazor) | `docker-compose up --build sample_blazor` + http://localhost:5005/ |
| [Tutorial](docs/tutorial/README.md) | `docker-compose up --build tutorial` + https://localhost:50005/README.md |


## Useful Links

* Check out [Stl.Fusion repository](https://github.com/servicetitan/Stl.Fusion) 
* Read the [Documentation](https://github.com/servicetitan/Stl.Fusion/blob/master/docs/README.md)
* Join our [Discord Server](https://discord.gg/EKEwv6d) to ask questions and track project updates.
