# Part 7: Scaling Fusion Services

Scaling Fusion services is actually simpler than it may seem 
at first - and mostly, you should take into account two 
key factors:

1.  You should treat any Fusion-based service host as one of your
    caching servers and scale it accordingly - but most importantly
    you should ensure these servers share a limited subset of data.
    The more data they share, the less is the cache hit ratio on a given
    server.

2.  You need to ensure that invalidations from any of such
    servers are "replicated" on any other server that may serve
    the same piece of data.

Let's look at a few specific cases to understand this better.

## Multi-Tenant Services

This is the simplest case. Let's assume we're building a Fusion-based
multi-tenant service, and:
- There are `H` hosts (VMs, containers, etc.) running the service
- We want to ensure each tenant can be served by at least `K` machines.

The simplest way to achieve a desirable distribution of
load in this case is to use 
[Consistent Hashing](https://en.wikipedia.org/wiki/Consistent_hashing).
or [Rendezvous Hashing](https://medium.com/i0exception/rendezvous-hashing-8c00e2fb58b0).
I'll use the later to show how to pick the host in this case.

First, let's assume `K == 1`. In this case we can pick our host using this logic:

```cs
Host GetHost(string tenantId) 
    => Hosts
        .Select(host => (
            Host: host, 
            Weight: Hash(host.Id, tenantId)
        ))
        .OrderBy(p.Weight) // Doesn't matter if it's descending or ascending
        .Select(p => p.Host)
        .First();
```

I wrote pretty inefficient code above mostly to make it easier 
to show the next step. If `K > 1`, we need to add a single line
to evenly distribute the load for every tenant to `K` machines
and still have a consistent mapping:

```cs
Host GetHost(string tenantId, string userIdOrIP) 
    => Hosts
        .Select(host => (
            Host: host, 
            Weight: Hash(host.Id, tenantId)
        ))
        .OrderBy(p.Weight)
        .Select(p => p.Host)
        .Skip(Hash(userIdOrIP) % K) // This is the only added line
        .First();
```

Now, it's clear this code is pretty inefficient - its time complexity 
is `O(N*log(N))`, but notice that while your set of hosts is stable,
you can cache the following list per each tenant to reduce the complexity
to `O(1)`:

```cs
Host[] GetCandidateHosts(string tenantIp) 
    => Hosts
        .Select(host => (
            Host: host, 
            Weight: Hash(host.Id, tenantId)
        ))
        .OrderBy(p.Weight)
        .Select(p => p.Host)
        .Take(K)
        .ToArray();
```

In practice, such a load balancing can be implemented by either
having this logic on your own proxy - and you can use e.g.
[AspNetCore.Proxy](https://github.com/twitchax/AspNetCore.Proxy)
or [YARP](https://devblogs.microsoft.com/dotnet/introducing-yarp-preview-1/)
to implement it.

Almost any industry standard load balancer supports consistent hash-based mapping 
too - in particular, you can use:
* [`hash` directive](http://nginx.org/en/docs/stream/ngx_stream_upstream_module.html#hash)
   on NGINX (it supports only `K == 1` scenario)
* [`hash-type consistent`](https://www.haproxy.com/blog/haproxys-load-balancing-algorithm-for-static-content-delivery-with-varnish/) 
  or [dynamically updated maps](https://www.haproxy.com/blog/introduction-to-haproxy-maps/)
  on HAProxy.
  
... To be continued.


#### [Next: Part 8 &raquo;](./Part08.md) | [Tutorial Home](./README.md)

