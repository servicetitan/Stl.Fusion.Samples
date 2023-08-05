# Part 0: NuGet packages

All Fusion packages are
[available on NuGet](https://www.nuget.org/packages?q=Owner%3Aservicetitan+Tags%3Astl_fusion):\
[![Build](https://github.com/servicetitan/Stl.Fusion/workflows/Build/badge.svg)](https://github.com/servicetitan/Stl.Fusion/actions?query=workflow%3A%22Build%22)
[![NuGetVersion](https://img.shields.io/nuget/v/Stl.Fusion)](https://www.nuget.org/packages?q=Owner%3Aservicetitan+Tags%3Astl_fusion)

Your should reference:

* `Stl.Fusion.Server` &ndash; from server-side assemblies
  * If you use .NET Framework 4.X, reference `Stl.Fusion.Server.NetFx` instead
* `Stl.Fusion.Client` &ndash; from client-side assemblies;
  * Blazor clients may reference `Stl.Fusion.Blazor` instead,
    which references `Stl.Fusion.Client`
* `Stl.Fusion` &ndash; from shared assemblies,
  i.e. the ones to be used on both sides.
* `Stl.Fusion.EntityFramework` &ndash; from server-side assemblies,
  if you plan to use [EF Core](https://docs.microsoft.com/en-us/ef/).

The list of Fusion packages:

* [Stl](https://www.nuget.org/packages/Stl/) - stands for "ServiceTitan Library"
  (yeah, every company needs its own [STL](https://en.wikipedia.org/wiki/Standard_Template_Library)).
  It's a collection of relatively isolated abstractions and helpers we couldn't find in BCL.
  `Stl` depends on [Castle.Core](https://www.nuget.org/packages/Castle.Core/) & maybe some other
  third-party packages.
* [Stl.Generators](https://www.nuget.org/packages/Stl.Generators/) - has no dependencies.
  It's a Roslyn-based code generation library focused on proxies / call interception.
  All Fusion proxies are implemented with it. 
* [Stl.Interception](https://www.nuget.org/packages/Stl.Interception/) - depends on `Stl`.
  Implements a number of call interception helpers which are used by [Stl.Generators].
* [Stl.Rpc](https://www.nuget.org/packages/Stl.Rpc/) - depends on `Stl`.
  An RPC API that Fusion uses to implement Compute Service Clients.
  It's probably the fastest RPC implementation over WebSockets that's currently available on .NET - even for plain RPC calls.
* [Stl.Rpc.Server](https://www.nuget.org/packages/Stl.Rpc.Server/) - depends on `Stl.Rpc`.
  An implementation of `Stl.Rpc` server for ASP.NET Core, which uses WebSockets.
* [Stl.Rpc.Server.NetFx](https://www.nuget.org/packages/Stl.Rpc.Server.NetFx/) - depends on `Stl.Rpc`.
  An implementation of `Stl.Rpc` server for ASP.NET / .NET Framework 4.X, which uses WebSockets.
* [Stl.CommandR](https://www.nuget.org/packages/Stl.CommandR/) - depends on `Stl` and `Stl.Interception`.
  CommandR is "[MediatR](hhttps://github.com/jbogard/MediatR) on steroids" designed to support
  not only interface-based command handlers, but also AOP-style handlers written as
  regular methods. Besides that, it unifies command handler API (pipeline behaviors and handlers
  are the same there) and helps to eliminate nearly all boilerplate code you'd have otherwise.
* [Stl.Fusion](https://www.nuget.org/packages/Stl.Fusion/) - depends on `Stl`, `Stl.Interception`, and `Stl.CommandR`.
  Nearly everything related to Fusion is there.
* [Stl.Fusion.Ext.Contracts](https://www.nuget.org/packages/Stl.Fusion.Ext.Contracts/) - depends on `Stl.Fusion`.
  Contracts for some handy extensions (ready-to-use Fusion services) - e.g. Fusion-based authentication is there.
* [Stl.Fusion.Ext.Services](https://www.nuget.org/packages/Stl.Fusion.Ext.Services/) - depends on `Stl.Fusion.Ext.Contracts` and `Stl.Fusion.EntityFramework`.
  Implementations of extension contracts from `Stl.Fusion.Ext.Contracts`.
* [Stl.Fusion.Server](https://www.nuget.org/packages/Stl.Fusion.Server/) - depends on `Stl.Fusion` and `Stl.Rpc`.
  Basically, Fusion + `Stl.Rpc.Server` + some handy server-side helpers.
* [Stl.Fusion.Server.NetFx](https://www.nuget.org/packages/Stl.Fusion.Server.NetFx/) -
  .NET Framework 4.X version of `Stl.Fusion.Server`.
* [Stl.Fusion.Blazor](https://www.nuget.org/packages/Stl.Fusion.Blazor/) - depends on `Stl.Fusion`.
  Provides Blazor-Fusion integration. Most importantly, there is `StatefulCompontentBase<TState>`,
  which allows to create auto-updating components which recompute their state once the data they consume
  from Fusion services changes.
* [Stl.Fusion.Blazor.Authentication](https://www.nuget.org/packages/Stl.Fusion.Blazor.Authentication/) - depends on `Stl.Fusion.Blazor` and `Stl.Fusion.Ext.Contracts`.
  Implements Fusion authentication-related Blazor components.
* [Stl.Fusion.EntityFramework](https://www.nuget.org/packages/Stl.Fusion.EntityFramework/) - depends on `Stl.Fusion`.
  Contains [EF Core](https://docs.microsoft.com/en-us/ef/) integrations for CommandR and Fusion.
* [Stl.Fusion.EntityFramework.Npgsql](https://www.nuget.org/packages/Stl.Fusion.EntityFramework.Npgsql/) -
  depends on `Stl.Fusion.EntityFramework`.  
  Contains [Npgsql](https://www.npgsql.org/) - based implementation of operation log change tracking.
  PostgreSQL has [`NOTIFY / LISTEN`](https://www.postgresql.org/docs/13/sql-notify.html)
  commands allowing to use it as a message queue, so if you use this database,
  you don't need a separate message queue to allow Fusion to notify peer hosts about
  operation log changes.
* [Stl.Fusion.EntityFramework.Redis](https://www.nuget.org/packages/Stl.Fusion.EntityFramework.Redis/) -
  depends on `Stl.Fusion.EntityFramework`.  
  Contains [Redis](https://redis.com/) - based implementation of operation log change tracking.

There are some other packages, but more likely than not you won't need them. 
The complete list can be found here (the packages with the most recent version aren't obsolete): 
- https://www.nuget.org/packages?q=Tags%3A%22stl_fusion%22 


#### [Next: Part 1 &raquo;](./Part01.md) | [Tutorial Home](./README.md)
