![](docs/img/Banner.jpg)

**Stl.Fusion.Samples** is a collection of samples for [Stl.Fusion](https://github.com/servicetitan/Stl.Fusion).

> All project updates are published on our [Discord Server](https://discord.gg/EKEwv6d); it's also the best place for Q/A.\
> [![Build](https://github.com/servicetitan/Stl.Fusion.Samples/workflows/Build/badge.svg)](https://github.com/servicetitan/Stl.Extras/actions?query=workflow%3A%22Build%22)

## Tutorial ##

* You can browse it [here](docs/tutorial/README.md)

## Blazor Sample ###

Features:
* "Server Time" and "Server Screen" - show simplest timeout-based invalidation
* "Chat" - a tiny chat app that shows how to properly invalidate parts of the state 
  on changes
* "Composition" shows Fusion's ability to compose the state by using both 
  local and remote parts.

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

### Running Blazor Sample

From Visual Studio or Rider:
* Open the `Samples.sln` in your favorite IDE
(note that Blazor *debugging* is currently supported only in Visual Studio and VSCode though)
* Run `Samples.Blazor.Server` project
* Open http://localhost:5005.

Using command line (Windows, Unix, Mac OS):
```
dotnet build
cd src/Blazor/Server
dotnet run
```

Alternatively, you can run this sample from Docker:
```cmd
docker-compose up sample_blazor
```

## Console Sample ###

TBD.
