![](docs/img/Banner.jpg)

Welcome to a collection of samples for [Stl.Fusion](https://github.com/servicetitan/Stl.Fusion)!

> All project updates are published on our [Discord Server](https://discord.gg/EKEwv6d); it's also the best place for Q/A.\
> [![Build](https://github.com/servicetitan/Stl.Fusion.Samples/workflows/Build/badge.svg)](https://github.com/servicetitan/Stl.Extras/actions?query=workflow%3A%22Build%22)

## Running Samples

The simplest way:
- Install [Docker](https://docs.docker.com/get-docker/) and
  [Docker Compose](https://docs.docker.com/compose/install/)
- To run [Blazor Samples](src/Blazor):
  1. Run `docker-compose up --build sample_blazor` in the root folder of this repository
  2. Open http://localhost:5005/.
- To run [Tutorial](docs/tutorial/README.md):
  1. Run `docker-compose up --build tutorial` in the root folder of this repository
  2. Open https://localhost:50005/README.md.

And if you'd rather run everything locally:
- Install the latest [.NET Core SDK 3.1](https://dotnet.microsoft.com/download)
- To run [Blazor Samples](src/Blazor):
  1. Run `dotnet run --project src/Blazor/Server/Samples.Blazor.Server.csproj`
  2. Open http://localhost:5005/.
- To run [Tutorial](docs/tutorial/README.md):
  1. Install [Try .NET](https://github.com/dotnet/try/blob/master/DotNetTryLocal.md).
     Its release version may fail to run the code, so we recommend to install 
     the preview version of this tool.
  2. Run `dotnet try --port 50005 docs/tutorial` in the root folder of this repository
  3. Open https://localhost:50005/README.md.

## What's Inside?

### 1. Tutorial

It's interactive &ndash; you can simply [browse it](docs/tutorial/README.md), but to
modify and run the C# code presented there, you need
[Try .NET](https://github.com/dotnet/try/blob/master/DotNetTryLocal.md)
or [Docker](https://www.docker.com/).

### 2. Blazor Samples

It's a [Blazor WebAssembly](https://devblogs.microsoft.com/aspnet/blazor-webassembly-3-2-0-now-available/)-based SPA hosted by
[ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet) website,
which also serves its API. The application includes:
* "Server Time" and "Server Screen" pages showing the simplest timeout-based invalidation
* "Chat" - a tiny chat relying on event-based invalidation
* "Composition" shows Fusion's ability to compose the state by using both 
  local `IComputed<T>` instances and client-side replicas of similar remote instances.

![](docs/img/Samples-Blazor.gif)

Notice that `Samples.Blazor.Server` exposes a regular RESTful API -
try invoking any of endpoints there right from embedded Swagger console.

![](docs/img/SwaggerDoc.jpg)

Besides that, "Composition" sample uses two different ways of composing the
state used by left and right panes:
  * The left pane uses the state
    [composed on the server-side](https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/src/Blazor/Server/Services/ServerSideComposerService.cs);
    its replica is used by the client
  * The right pane uses the state
    [composed completely on the client-side](https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/src/Blazor/Client/Services/ClientSideComposerService.cs) 
    by combining other server-side replicas.
  * **The surprising part:** two above files are almost identical!

## Useful Links

* Check out [Stl.Fusion repository](https://github.com/servicetitan/Stl.Fusion) 
* Read the [Documentation](https://github.com/servicetitan/Stl.Fusion/blob/master/docs/README.md)
* Join our [Discord Server](https://discord.gg/EKEwv6d) to ask questions and track project updates.
