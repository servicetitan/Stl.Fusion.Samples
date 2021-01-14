# Part 0: NuGet packages

All Fusion packages are
[available on NuGet](https://www.nuget.org/packages?q=Owner%3Aservicetitan+Tags%3Astl_fusion):\
[![Build](https://github.com/servicetitan/Stl.Fusion/workflows/Build/badge.svg)](https://github.com/servicetitan/Stl.Fusion/actions?query=workflow%3A%22Build%22)
[![NuGetVersion](https://img.shields.io/nuget/v/Stl.Fusion)](https://www.nuget.org/packages?q=Owner%3Aservicetitan+Tags%3Astl_fusion)

Your should reference:

* [Stl.Fusion.Server](https://www.nuget.org/packages/Stl.Fusion.Server/) &ndash; from server-side assemblies
* [Stl.Fusion.Client](https://www.nuget.org/packages/Stl.Fusion.Client/) &ndash; from client-side assemblies;
  Blazor clients may reference [Stl.Fusion.Blazor](https://www.nuget.org/packages/Stl.Fusion.Blazor/) instead
* [Stl.Fusion](https://www.nuget.org/packages/Stl.Fusion/) &ndash; from shared assemblies,
  i.e. the ones to be used on both sides.
* [Stl.Fusion.EntityFramework](https://www.nuget.org/packages/Stl.Fusion.EntityFramework/) &ndash; from server-side assemblies, 
  if you plan to use [EF Core](https://docs.microsoft.com/en-us/ef/). Most likely you'll need some of helper types it provides.

The full list of Fusion packages:

* [Stl](https://www.nuget.org/packages/Stl/) - stands for "ServiceTitan Library"
  (yeah, every company needs its own [STL](https://en.wikipedia.org/wiki/Standard_Template_Library)).
  It's a collection of relatively isolated abstractions and helpers we couldn't find in BCL.
  `Stl` depends on [Castle.Core](https://www.nuget.org/packages/Castle.Core/) & maybe some other
  third-party packages.
* [Stl.Net](https://www.nuget.org/packages/Stl.Net/) - depends on `Stl`.
  [WebSocketChannel](https://github.com/servicetitan/Stl.Fusion/blob/master/src/Stl.Net/WebSocketChannel.cs)
  is currently the only type it contains.  
* [Stl.Interception](https://www.nuget.org/packages/Stl.Interception/) - depends on `Stl`.
  Call interception helpers based on [Castle DynamicProxy](http://www.castleproject.org/projects/dynamicproxy/).
* [Stl.CommandR](https://www.nuget.org/packages/Stl.CommandR/) - depends on `Stl` and `Stl.Interception`.
  CommandR is "[MediatR](hhttps://github.com/jbogard/MediatR) on steroids" designed to support
  not only interface-based command handlers, but also AOP-style handlers written as 
  regular methods. Besides that, it unifies command handler API (pipeline behaviors and handlers 
  are the same there) and helps to eliminate nearly all boilerplate code you'd have otherwise.
* [Stl.Fusion](https://www.nuget.org/packages/Stl.Fusion/) - depends on `Stl`, `Stl.Interception`, and `Stl.CommandR`.
  Nearly everything related to Fusion is there.
* [Stl.Fusion.Server](https://www.nuget.org/packages/Stl.Fusion.Server/) - depends on `Stl.Fusion` and `Stl.Net`.
  It implements server-side WebSocket endpoint allowing client-side counterpart to communicate
  with Fusion `Publisher`. In addition, it provides a base class for fusion API controllers
  (`FusionController`) and a few extension methods helping to register all of that in your web app.
* [Stl.Fusion.Client](https://www.nuget.org/packages/Stl.Fusion.Client/) - depends on `Stl.Fusion` and `Stl.Net`.
  Implements a client-side WebSocket communication channel and
  [RestEase](https://github.com/canton7/RestEase) - based API client builder compatible with
  `FusionControler`-based API endpoints. All of that together allows you to get computed
  instances on the client that "mirror" their server-side counterparts.
* [Stl.Fusion.Blazor](https://www.nuget.org/packages/Stl.Fusion.Blazor/) - depends on `Stl.Fusion.Client`.
  Implements handy Blazor components. Currently there is `StatefulCompontentBase<TState>`
  and its 2 descendants: `LiveComponentBase<T>` and `LiveComponentBase<T, TLocals>`.
* [Stl.Fusion.EntityFramework](https://www.nuget.org/packages/Stl.Fusion.EntityFramework/) - depends on `Stl.Fusion`.
  Contains [EF Core](https://docs.microsoft.com/en-us/ef/)-related helpers for Fusion apps.
    

#### [Next: Part 1 &raquo;](./Part01.md) | [Tutorial Home](./README.md)

