# Stl.Fusion Tutorial

> This tutorial is interactive &ndash; you can simply browse it, but you won't
> be able to see the output of C# fragments, modify, and run them. 
> [Try .NET](https://github.com/dotnet/try/blob/master/DotNetTryLocal.md)
> or [Docker](https://www.docker.com/) is required to run it in interactive mode.

The simplest way to run this tutorial:
- Install [Docker](https://docs.docker.com/get-docker/) and
  [Docker Compose](https://docs.docker.com/compose/install/)
- Run `docker-compose up tutorial` in the root folder of this repository
- Open https://localhost:50005/README.md.

Alternatively, you can run it with `dotnet try` CLI tool:
- Install the latest version of [.NET Core SDK 3.1](https://dotnet.microsoft.com/download)
- Install [Try .NET](https://github.com/dotnet/try/blob/master/DotNetTryLocal.md) -
  it's the tool to "run" the tutorial. Its release version may fail to run
  the code, so we recommend to install the preview version of this tool.
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
* [Part 1: `IComputed<TOut>` and `SimpleComputed<TOut>`](./Part01.md)
* [Part 2: Dependencies between computed instances](./Part02.md)
* [Part 3: `IComputedService` and a nicer way to create `IComputed<TOut>`](./Part03.md)
* [Part 4: Computed Services: execution, caching, and invalidation](./Part04.md)
* [Part 5: Computed Services: dependencies](./Part05.md)
* [Part 6: Computed Instances and Computed Services - Review](./Part06.md)
* Part 7+: To be added later this week.

