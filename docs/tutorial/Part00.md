# Part 0: NuGet packages

All Stl.Fusion packages are 
[available on NuGet](https://www.nuget.org/packages?q=Owner%3Aservicetitan+Tags%3Astl_fusion):\
[![Build](https://github.com/servicetitan/Stl.Fusion/workflows/Build/badge.svg)](https://github.com/servicetitan/Stl.Fusion/actions?query=workflow%3A%22Build%22)
[![NuGetVersion](https://img.shields.io/nuget/v/Stl.Fusion)](https://www.nuget.org/packages?q=Owner%3Aservicetitan+Tags%3Astl_fusion) 

Your should reference:

* [Stl.Fusion.Server](https://www.nuget.org/packages/Stl.Fusion.Server/) &ndash; from server-side assemblies 
* [Stl.Fusion.Client](https://www.nuget.org/packages/Stl.Fusion.Client/) &ndash; from client-side assemblies;
  Blazor clients may reference [Stl.Fusion.Blazor](https://www.nuget.org/packages/Stl.Fusion.Blazor/) instead
* [Stl.Fusion](https://www.nuget.org/packages/Stl.Fusion/) &ndash; from shared assemblies, 
  i.e. the ones to be used on both sides.

The full list of Fusion packages:

* [Stl](https://www.nuget.org/packages/Stl/) - stands for "ServiceTitan Library" 
  (yeah, every company needs its own [STL](https://en.wikipedia.org/wiki/Standard_Template_Library)).
  It's a collection of relatively isolated abstractions and helpers we couldn't find in BCL.
  `Stl` depends on [Castle.Core](https://www.nuget.org/packages/Castle.Core/) & maybe some other
  third-party packages.
* [Stl.Fusion](https://www.nuget.org/packages/Stl.Fusion/) - depends on `Stl`.
  Nearly everything related to Fusion is there.
* [Stl.Fusion.Server](https://www.nuget.org/packages/Stl.Fusion.Server/) - depends on `Stl.Fusion`.
  It implements server-side WebSocket endpoint allowing client-side counterpart to communicate
  with Fusion `Publisher`. In addition, it provides a base class for fusion API controllers
  (`FusionController`) and a few extension methods helping to register all of that in your web app.
* [Stl.Fusion.Client](https://www.nuget.org/packages/Stl.Fusion.Client/) - depends on `Stl.Fusion`.
  Implements a client-side WebSocket communication channel and
  [RestEase](https://github.com/canton7/RestEase) - based API client builder compatible with
  `FusionControler`-based API endpoints. All of that together allows you to get computed
  instances on the client that "mirror" their server-side counterparts.
* [Stl.Fusion.Blazor](https://www.nuget.org/packages/Stl.Fusion.Blazor/) - depends on `Stl.Fusion.Client`.
  Currently there are just two types - `LiveComponentBase<TState>` and
  `LiveComponentBase<TLocal, TState>`. These are base classes for your own Blazor components
  capable of updating their state in real time relying on `ILiveState`.

#### [Next: Part 1 &raquo;](./Part01.md) | [Tutorial Home](./README.md)

