Welcome to the collection of [Fusion] samples!

> All project updates are published on its [Discord Server]; it's also the best place for Q/A.\
> [![Build](https://github.com/servicetitan/Stl.Fusion.Samples/workflows/Build/badge.svg)](https://github.com/servicetitan/Stl.Fusion.Samples/actions?query=workflow%3A%22Build%22)
> [![Discord Server](https://img.shields.io/discord/729970863419424788.svg)](https://discord.gg/EKEwv6d)  

Don't know what Fusion is? [You should!](https://github.com/servicetitan/Stl.Fusion) 
Fusion is your #1 partner in crime if you're 
building a real-time app (an online app delivering
some or all updates in real-time) or a high-load app.
Moreover, it plays really well with Blazor and works on MAUI.

Fusion allows you to build fully real-time UIs like this one - 
and it's nearly as easy as if there is no logic related to real-time
updates at all:

![](docs/img/Samples-Blazor.gif)

<img src="https://img.shields.io/badge/-Live!-red" valign="middle"> Play with [live version of this app](https://fusion-samples.servicetitan.com) right now!

<img src="https://img.shields.io/badge/-New!-green" valign="middle"> If you're curious about what Fusion allows you to build, check out [Actual Chat] - a new chat app created to maximize the efficiency of your communication with the modern AI.

- Currently it does what most of other chats do, but also allows you to **talk in chats in real-time**. When you do this, it delivers your voice and its transcription to every person reading or listening, which allows other people to join the conversation as soon as they see its very first words, or read/replay it later.
- There are SSB/WASM, iOS, Android, and Windows clients, with nearly 100% of the code shared among them.
- Free to ask your Fusion-related questions here: https://actual.chat/chat/zu3UA03jvX
- As far as we know, it's one of the most complex Fusion-based apps. The startup behind Actual Chat was founded by Fusion's author.


## Running Samples

Build & run locally with [.NET 8.0 Preview SDK](https://dotnet.microsoft.com/download):

```bash
# Run this command first
dotnet build
```

| Sample | Command |
|-|-|
| [HelloCart] | `dotnet run -p src/HelloCart/HelloCart.csproj` |
| [HelloWorld] | `dotnet run -p src/HelloWorld/HelloWorld.csproj` |
| [HelloBlazorServer] |  `dotnet run -p src/HelloBlazorServer/HelloBlazorServer.csproj` + open http://localhost:5005/ |
| [HelloBlazorHybrid] |  `dotnet run -p src/HelloBlazorHybrid/Server/Server.csproj` + open http://localhost:5005/ |
| [Blazor Samples] |  `dotnet run -p src/Blazor/Server/Server.csproj` + open http://localhost:5005/ |
| [MiniRpc] | `dotnet run -p src/MiniRpc/MiniRpc.csproj` |
| [MultiServerRpc] | `dotnet run -p src/MultiServerRpc/MultiServerRpc.csproj ` |
| [Benchmark] | `dotnet run -c:Release -p src/Benchmark/Benchmark.csproj` |
| [RpcBenchmark] | `dotnet run -c:Release -p src/RpcBenchmark/RpcBenchmark.csproj` |
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
| [MiniRpc] | `docker-compose run sample_mini_rpc dotnet Samples.MiniRpc.dll` |
| [MultiServerRpc] | `docker-compose run sample_multi_server_rpc dotnet Samples.MultiServerRpc.dll` |
| [Benchmark] | `docker-compose run sample_benchmark dotnet Samples.Benchmark.dll` |
| [RpcBenchmark] | `docker-compose run sample_rpc_benchmark dotnet Samples.RpcBenchmark.dll` |
| [Tutorial] | `docker-compose run --service-ports tutorial` + open https://localhost:50005/README.md |

A detailed description of nearly every sample can be found here: https://servicetitan.github.io/Stl.Fusion.Samples/

## Useful Links

* Check out [Fusion repository on GitHub]
* Play with [Actual Chat] - a chat app with a mission to improve the quality of your online communication with the modern AI. It allows you to join or initiate a conversation much faster by delivering your voice and its transcription in real-time and follow other people's voice conversations even if you're late to join them. Check it out - it's a fun experience. And feel free to ask your Fusion-related questions here: https://actual.chat/chat/zu3UA03jvX
* Join our [Discord Server] to ask questions and track project updates. We'll migrate it to [Actual Chat] as soon as it gets "Places" feature (~ Servers in Discord) 😉
* Go to [Documentation Home].

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
[MiniRpc]: src/MiniRpc
[MultiServerRpc]: src/MultiServerRpc
[Benchmark]: src/Benchmark
[RpcBenchmark]: src/RpcBenchmark
[Tutorial]: docs/tutorial/README.md
[Fusion Tutorial]: docs/tutorial/README.md
[Documentation Home]: https://github.com/servicetitan/Stl.Fusion/blob/master/docs/README.md
[Actual Chat]: https://actual.chat

[Compute Services]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part01.md
[Compute Service]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part01.md
[`IComputed<T>`]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part02.md
[Computed Value]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part02.md
[Live State]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part03.md
[Replica Services]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part04.md
[Compute Service Clients]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/docs/tutorial/Part04.md
[Fusion In Simple Terms]: https://medium.com/@alexyakunin/stl-fusion-in-simple-terms-65b1975967ab?source=friends_link&sk=04e73e75a52768cf7c3330744a9b1e38

[Discord Server]: https://discord.gg/EKEwv6d
[Fusion Feedback Form]: https://forms.gle/TpGkmTZttukhDMRB6
