Welcome to a collection of [Fusion] samples!

> All project updates are published on its [Discord Server]; it's also the best place for Q/A.\
> [![Build](https://github.com/servicetitan/Stl.Fusion.Samples/workflows/Build/badge.svg)](https://github.com/servicetitan/Stl.Fusion.Samples/actions?query=workflow%3A%22Build%22)
> [![Discord Server](https://img.shields.io/discord/729970863419424788.svg)](https://discord.gg/EKEwv6d)  

## What's Inside?

### 0. Solution Templates

We don't provide `dotnet new`-based templates yet, but you can find
template solutions to copy in [`templates/*` folders](./templates).

![](img/Samples-Template.gif)

Templates are also included into `Samples.sln`, so you can
try any of them by opening this solution & running one of 
template `.csproj` files.

### 1. HelloCart and HelloWorld Samples

![](img/Samples-HelloCart.gif)

<img src="https://img.shields.io/badge/-New!-brightgreen" valign="middle"> [HelloCart] 
is a small console app designed to show how to implement a simple 
Fusion API by starting from a toy version of it
and gradually transition to its production-ready version
that uses EF Core, can be called remotely, and scales 
horizontally relying on multi-host invalidation.

["QuickStart: Learn 80% of Fusion by walking through HelloCart sample"](./tutorial/QuickStart.md) is the newest part of [Fusion Tutorial] that covers
specifically this sample. Check it out!

And [HelloWorld] shows how to create
an incremental build simulation on Fusion. Nothing is really 
built there, of course - the goal is to shows how Fusion
"captures" dependencies right when you use them and runs
cascading invalidations.

If you're choosing between `HelloWorld` and `HelloCart` - 
play with `HelloCart` first. It is also a sample covered
in [QuickStart part](tutorial/QuickStart.md) 
of the [Fusion Tutorial].

### 2. HelloBlazorServer and HelloBlazorHybrid Samples

[HelloBlazorServer] is the default Blazor Server App 
modified to reveal some Fusion powers. Contrary to the original app:
* It displays changes made to a *global* counter in real-time
* Similarly, it updates weather forecasts in real-time
* A new "Simple Chat" sample shows a bit more complex update scenario and
  features a simple chat bot.

![](img/Samples-HelloBlazorServer.gif)

If you're curious how big is the difference between the code of
these samples and a similar code without any real-time
features, 
[check out this part of Fusion README.md](https://github.com/servicetitan/Stl.Fusion#enough-talk---lets-fight-show-me-the-code).

[HelloBlazorHybrid] is the same sample, but modified to support both
Blazor Server and Blazor WebAssembly modes.

### 3. Blazor Samples

<img src="https://img.shields.io/badge/-Live!-red" valign="middle"> Play with [live version of this app](https://fusion-samples.servicetitan.com) right now!

It's a dual-mode [Blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-3.1) SPA hosted by
[ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet) website,
which also serves its API. The application includes:
* "Server Time" and "Server Screen" pages showing the simplest timeout-based invalidation
* "Chat" &ndash; a tiny chat relying on event-based invalidation
* "Composition" shows how Fusion tracks and updates a complex state built 
  from the output of [Compute Services] (local producers) and 
  [Replica Services] (remote producers)
* "Authentication" &ndash; a GitHub authentication sample with Google-style real-time 
  session tracking, "Kick", and "Sign-out everywhere" actions.

> Check out a [7-min. video walk-through](https://www.youtube.com/watch?v=nBJo9Y2TvEo) 
> for this sample - the animations below show just some of its features.

![](img/Samples-Blazor.gif)

Note that "Composition" sample shown in a separate window in the bottom-right corner
also properly updates everything. It shows Fusion's ability to use both local `IComputed<T>` 
instances and client-side replicas of similar server-side instances to compute a new value
that properly tracks all these dependencies and updates accordingly: 
* First panel's UI model is 
  [composed on the server-side](https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/src/Blazor/Server/Services/ComposerService.cs);
  its client-side replica is bound to the component displaying the panel
* And the second panel uses an UI model
  [composed completely on the client](https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/src/Blazor/UI/Services/LocalComposerService.cs) 
  by combining server-side replicas of all the values used there.
* **The surprising part:** two above files are almost identical!

The sample supports **both (!)** Server-Side Blazor and Blazor WebAssembly modes &ndash;
you can switch the mode on its "Home" page.

![](img/Samples-Blazor-Auth.gif)

Moreover, it also exposes a regular RESTful API &ndash;
try invoking any of endpoints there right from embedded Swagger console.

![](img/SwaggerDoc.jpg)

### 4. Caching Sample

It's a console app running the benchmark (`Client`) + ASP.NET Core API `Server`. Its output on Ryzen Threadripper 3960X:

```text
Local services:
Fusion's Compute Service [-> EF Core -> SQL Server]:
  Reads         : 27.55M operations/s
Regular Service [-> EF Core -> SQL Server]:
  Reads         : 25.05K operations/s

Remote services:
Fusion's Replica Client [-> HTTP+WebSocket -> ASP.NET Core -> Compute Service -> EF Core -> SQL Server]:
  Reads         : 20.29M operations/s
RestEase Client [-> HTTP -> ASP.NET Core -> Compute Service -> EF Core -> SQL Server]:
  Reads         : 127.96K operations/s
RestEase Client [-> HTTP -> ASP.NET Core -> Regular Service -> EF Core -> SQL Server]:
  Reads         : 20.46K operations/s
```

What's interesting in this output?
- Fusion-backed API endpoint serving relatively small amount of cacheable data
    scales to ~ **130,000 RPS** while running the test on the same machine 
    (that's a disadvantage).
- Identical EF Core-based API endpoint scales to just 20K RPS.

So there is a ~ 6.5x difference for an extremely simple EF Core service 
hitting a tiny DB running in simple recovery mode.
In other words, use of Fusion on server-side only brings ~ an order of 
magnitude performance boost even when there is almost nothing to speed up! 

Besides that, the test shows [Replica Services] scale ~ almost as local 
[Compute Services], i.e. to ~ **20 million "RPS"**. 
These aren't true RPS, of course - Replica Service simply kills any RPC 
for cached values that are known to be consistent. But nevertheless,
it's still a pretty unique feature Fusion brings to the table &ndash; and that's
exactly what allows it Blazor samples to share the same code for both WASM and SSB
modes. So even though Replica Service is just a client for remote Compute Service,
its performance is very similar!

### 5. Tutorial

It's interactive &ndash; you can simply [browse it](tutorial/README.md), but to
modify and run the C# code presented there, you need
[Try .NET](https://github.com/dotnet/try/blob/master/DotNetTryLocal.md)
or [Docker](https://www.docker.com/).

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

## Useful Links

* Check out [Fusion repository on GitHub]
* Go to [Documentation Home]
* Explore [Board Games](https://github.com/alexyakunin/BoardGames) -  a real-time multiplayer board gaming app built on Fusion
* Join our [Discord Server] to ask questions and track project updates.

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
[Tutorial]: tutorial/README.md
[Fusion Tutorial]: tutorial/README.md
[Documentation Home]: https://github.com/servicetitan/Stl.Fusion/blob/master/docs/README.md

[Compute Services]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/tutorial/Part01.md
[Compute Service]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/tutorial/Part01.md
[`IComputed<T>`]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/tutorial/Part02.md
[Computed Value]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/tutorial/Part02.md
[Live State]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/tutorial/Part03.md
[Replica Services]: https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/tutorial/Part04.md
[Fusion In Simple Terms]: https://medium.com/@alexyakunin/stl-fusion-in-simple-terms-65b1975967ab?source=friends_link&sk=04e73e75a52768cf7c3330744a9b1e38

[Discord Server]: https://discord.gg/EKEwv6d
[Fusion Feedback Form]: https://forms.gle/TpGkmTZttukhDMRB6
