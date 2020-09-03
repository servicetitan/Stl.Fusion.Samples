# Stl.Fusion Tutorial

> This tutorial is interactive &ndash; you can simply browse it,
> but to modify and run the C# code presented here, you need
> [Try .NET](https://github.com/dotnet/try/blob/master/DotNetTryLocal.md)
> or [Docker](https://www.docker.com/).

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

> We highly recommended you to read the
> [Overview](https://github.com/servicetitan/Stl.Fusion/blob/master/docs/Overview.md) first.

The code based on `Stl.Fusion` (we'll refer to it as "Fusion" further)
might look completely weird at first - that's because it is based
on abstractions you need to learn about before starting
to dig into the code.

Understanding how they work will also eliminate a lot
of questions you might get further, so we highly recommend you
to complete this tutorial *before* digging into the source
code of Fusion samples.

Without further ado:

* [Part 0: NuGet packages](./Part00.md)
* [Part 1: Compute Services](./Part01.md)
* [Part 2: Computed Values: IComputed&lt;T&gt;](./Part02.md)
* [Part 3: State: IState&lt;T&gt; and its flavors](./Part03.md)

To be added soon:

* [Part 4: Replica Services](./Part04.md)
* [Part 5: Use case: Transparent In-Process Caching + Swapping to External Cache](./Part05.md)
* [Part 6: Use case: Server-Side Blazor (SSB) Apps](./Part06.md)
* [Part 7: Use case: Blazor WebAssembly Apps](./Part07.md)
* [Part 8: Use case: Hybrid Blazor (SSB + WASM) Apps](./Part08.md)

Join our [Discord Server](https://discord.gg/EKEwv6d)
to ask questions and track project updates.

