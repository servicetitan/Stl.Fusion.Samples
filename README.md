Welcome to the collection of [Fusion] samples!

> All project updates are published on its [Discord Server]; it's also the best place for Q/A.\
> [![Build](https://github.com/servicetitan/Stl.Fusion.Samples/workflows/Build/badge.svg)](https://github.com/servicetitan/Stl.Fusion.Samples/actions?query=workflow%3A%22Build%22)
> [![Discord Server](https://img.shields.io/discord/729970863419424788.svg)](https://discord.gg/EKEwv6d)  

Don't know what Fusion is? [You should!](https://github.com/servicetitan/Stl.Fusion) 
Fusion is probably your #1 partner in crime if you're 
building a real-time app (an online app delivering
some or all updates in real-time) or a high-load app,
and moreover, it plays really well with Blazor.

In particular, Fusion allows you to build fully 
real-time UIs like this one as easily as if there would
be no logic related to real-time updates at all:

![](docs/img/Samples-Blazor.gif)

<img src="https://img.shields.io/badge/-Live!-red" valign="middle"> Play with [live version of this app](https://fusion-samples.servicetitan.com) right now!

If you're looking for detailed description of every sample,
see https://servicetitan.github.io/Stl.Fusion.Samples/

## Running Samples

Build & run locally with [.NET 6.0 SDK](https://dotnet.microsoft.com/download):

```bash
# Run this command first
dotnet build
```

| Sample | Command |
|-|-|
| [HelloCart] | `dotnet run -p src/HelloCart/HelloCart.csproj` |
| [HelloWorld] | `dotnet run -p src/HelloWorld/HelloWorld.csproj` |
| [HelloBlazorServer] |  `dotnet run --project src/HelloBlazorServer/HelloBlazorServer.csproj` + open http://localhost:5005/ |
| [HelloBlazorHybrid] |  `dotnet run --project src/HelloBlazorHybrid/Server/Server.csproj` + open http://localhost:5005/ |
| [Blazor Samples] |  `dotnet run --project src/Blazor/Server/Server.csproj` + open http://localhost:5005/ |
| [Caching] | `Run-Sample-Caching.cmd`. See [Run-Sample-Caching.cmd](Run-Sample-Caching.cmd) to run this sample on Unix. |
| [Tutorial] | [Install Try .NET](https://github.com/dotnet/try/blob/master/DotNetTryLocal.md) + `dotnet try --port 50005 docs/tutorial` |

Build & run with [Docker](https://docs.docker.com/get-docker/) + 
[Docker Compose](https://docs.docker.com/compose/install/):

```bash
# Run this command first
docker-compose build
```

| Sample | Command |
|-|-|
| [HelloCart] | `docker-compose run sample_hello_cart dotnet Samples.HelloCart.dll` |
| [HelloWorld] | `docker-compose run sample_hello_world dotnet Samples.HelloWorld.dll` |
| [HelloBlazorServer] | `docker-compose run --service-ports sample_hello_blazor_server` + open http://localhost:5005/ |
| [HelloBlazorHybrid] | `docker-compose run --service-ports sample_hello_blazor_hybrid` + open http://localhost:5005/ |
| [Blazor Samples] | `docker-compose run --service-ports sample_blazor` + open http://localhost:5005/ |
| [Caching] | `docker-compose run sample_caching_client dotnet Samples.Caching.Client.dll` |
| [Tutorial] | `docker-compose run --service-ports tutorial` + open https://localhost:50005/README.md |

## Samples' Documentation

See https://servicetitan.github.io/Stl.Fusion.Samples/

## Useful Links

* More detailed version of this page: https://servicetitan.github.io/Stl.Fusion.Samples/
* Check out [Fusion repository on GitHub]
* Go to [Documentation Home]
* Join our [Discord Server] to ask questions and track project updates
* Explore [Board Games](https://github.com/alexyakunin/BoardGames) -  a real-time multiplayer board gaming app built on Fusion.

**P.S.** If you've already spent some time learning about Fusion, 
please help us to make it better by completing [Fusion Feedback Form] 
(1&hellip;3 min).


[Fusion]: https://github.com/servicetitan/Stl.Fusion
[Fusion repository on GitHub]: https://github.com/servicetitan/Stl.Fusion

[HelloCart]: src/HelloCart
[HelloWorld]: src/HelloWorld
[HelloBlazorServer]: src/HelloBlazorServer
[HelloBlazorHybrid]: src/HelloBlazorHybrid
[Blazor Samples]: src/Blazor
[Caching]: src/Caching
[Tutorial]: docs/tutorial/README.md
[Fusion Tutorial]: docs/tutorial/README.md
[Documentation Home]: https://github.com/servicetitan/Stl.Fusion/blob/master/docs/README.md

[Compute Services]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part01.md
[Compute Service]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part01.md
[`IComputed<T>`]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part02.md
[Computed Value]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part02.md
[Live State]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part03.md
[Replica Services]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part04.md
[Fusion In Simple Terms]: https://medium.com/@alexyakunin/stl-fusion-in-simple-terms-65b1975967ab?source=friends_link&sk=04e73e75a52768cf7c3330744a9b1e38

[Discord Server]: https://discord.gg/EKEwv6d
[Fusion Feedback Form]: https://forms.gle/TpGkmTZttukhDMRB6
