> All project updates are published on our [Discord Server](https://discord.gg/EKEwv6d); it's also the best place for Q/A.\
> [![Build](https://github.com/servicetitan/Stl.Fusion.Samples/workflows/Build/badge.svg)](https://github.com/servicetitan/Stl.Extras/actions?query=workflow%3A%22Build%22)

**Stl.Fusion.Samples** is a collection of samples for [Stl.Fusion](https://github.com/servicetitan/Stl.Fusion). It includes:

### Blazor Sample ###

Features:
* "Server Time" and "Server Screen" - show simplest timeout-based invalidation
* "Chat" - a tiny chat app that shows how to properly invalidate parts of the state 
  on changes
* "Composition" - shows Fusion's ability to compose the state by using both 
  local and remote parts:
  * The state used by the left pane is
    [composed on the server side](https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/src/Blazor/Server/Services/ServerSideComposerService.cs);
    its replica is used by the client
  * And the one used by the right pane is
    [composed completely on the client](https://github.com/servicetitan/Stl.Fusion.Samples/blob/master/src/Blazor/Client/Services/ClientSideComposerService.cs) 
    by combining other server-side replicas.
  * **The surprising part:** two above files are almost identical!

![](./docs/img/Samples-Blazor.gif)

### Console Sample ###

TBD.
