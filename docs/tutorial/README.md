# Fusion Tutorial

> All project updates are published on [Gitter]; it's also the best place for Q/A.\
> [![Build](https://github.com/servicetitan/Stl.Fusion.Samples/workflows/Build/badge.svg)](https://github.com/servicetitan/Stl.Fusion.Samples/actions?query=workflow%3A%22Build%22)
> [![Gitter](https://badges.gitter.im/Stl-Fusion/community.svg)](https://gitter.im/Stl-Fusion/community)

This is an *interactive* tutorial for [Fusion] - a .NET Core library
trying to make real-time a new normal for any connected apps.
And although you can simply browse it, you can also run and modify any
C# code featured here. All you need is
[Try .NET](https://github.com/dotnet/try/blob/master/DotNetTryLocal.md)
or [Docker](https://www.docker.com/).

The simplest way to run this tutorial:

- Install [Docker](https://docs.docker.com/get-docker/) and
  [Docker Compose](https://docs.docker.com/compose/install/)
- Run `docker-compose up --build tutorial` in the root folder of this repository
- Open https://localhost:50005/README.md.

Alternatively, you can run it with `dotnet try` CLI tool:

- Install the latest [.NET Core SDK 3.1](https://dotnet.microsoft.com/download)
- Install [Try .NET](https://github.com/dotnet/try/blob/master/DotNetTryLocal.md).
  Its release version may fail to run the code, so we recommend to install
  the preview version of this tool.
- Run `dotnet try --port 50005 docs/tutorial` in the root folder of this repository
- Open https://localhost:50005/README.md.

## Tutorial

The code based on Fusion might look completely weird at first -
that's because it is based on abstractions you need to learn about
before starting to dig into the code.

Understanding how they work will also eliminate a lot
of questions you might get further, so we highly recommend you
to complete this tutorial *before* digging into the source
code of Fusion samples.

Without further ado:

* [Part 0: NuGet packages](./Part00.md)
* [Part 1: Compute Services](./Part01.md)
* [Part 2: Computed Values: IComputed&lt;T&gt;](./Part02.md)
* [Part 3: State: IState&lt;T&gt; and its flavors](./Part03.md)
* [Part 4: Replica Services](./Part04.md)

To be added soon:

* [Part 5: Use case: Transparent In-Process Caching + Swapping to External Caches](./Part05.md)
* [Part 6: Use case: Real-time UI in Server-Side Blazor (SSB) Apps](./Part06.md)
* [Part 7: Use case: Real-time UI in Blazor WebAssembly Apps](./Part07.md)
* [Part 8: Use case: Real-time UI in Hybrid Blazor (SSB + WASM) Apps](./Part08.md)
* [Part 9: Use case: Real-time UI in JS / React Apps](./Part09.md)

Check out the [Overview](https://github.com/servicetitan/Stl.Fusion/blob/master/docs/Overview.md)
as well - it provides a high-level description of Fusion abstractions.

Join our [Gitter Chat Room] or [Discord Server] to ask questions and track project updates.

[Gitter]: https://gitter.im/Stl-Fusion/community
[Gitter Chat Room]: https://gitter.im/Stl-Fusion/community
[Discord Server]: https://discord.gg/EKEwv6d
[Fusion Feedback Form]: https://forms.gle/TpGkmTZttukhDMRB6
