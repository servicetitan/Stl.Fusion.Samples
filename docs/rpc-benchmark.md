# RpcBenchmark

The benchmark measures the performance of mainstream web RPC libraries for .NET (RPC over HTTP / WebSockets), as well as the performance of the new contender - Stl.Rpc from [Fusion](https://github.com/servicetitan/Stl.Fusion).

<img src="https://img.shields.io/badge/-NOTE:-red" valign="middle"> RpcBenchmark is quite new, so there is a chance that our test setup isn't optimal for some of the tested libraries - even though we did a fair amount of research to maximize the throughput of each one of them. If you'll find any issues, please let us know / send us a Pull Request.


## Tested libraries

- [Stl.Rpc](https://www.nuget.org/packages/Stl.Rpc) &ndash; a communication library used by [Fusion](https://github.com/servicetitan/Stl.Fusion), which can be used independently as well.
- [SignalR](https://github.com/SignalR/SignalR)
- [StreamJsonRpc](https://github.com/microsoft/vs-streamjsonrpc)
- [MagicOnion](https://github.com/Cysharp/MagicOnion)
- [gRPC](https://grpc.io/) &ndash; you can find its [official benchmarking dashboard here](https://grpc.io/docs/guides/benchmarking/). There are multiple unofficial benchmarks as well, such as [this one](https://github.com/LesnyRumcajs/grpc_bench).
- HTTP: a RESTful ASP.NET Core API endpoint on the server side, and [RestEase](https://github.com/canton7/RestEase)-based  `HttpClient` wrapper on the client side.

## How RpcBenchmark works?

The test is designed to measure the overhead/inefficiencies. The overhead per call is ~ `1/callsPerSecond` assuming you're CPU-constrained, so so measure the efficiency of a given library, all you need is to squeeze as many `callsPerSecond` as possible.

Here is what RpcBenchmark does:
- It creates N clients and N * M worker tasks
- Each worker task runs a loop calling a single method on its client, quickly validates its result & counts the call
- There are 3 test workloads: `Sum`, `GetUser`, and `SayHello`. They differ only by the payload size:
  - `Sum` is the simplest one - `(int, int) -> int`
  - `GetUser` is `(long) -> User` (medium payload)
  - `SayHello` uses exactly the same payload as [in this gRPC benchmark](https://github.com/LesnyRumcajs/grpc_bench/tree/master/dotnet_grpc_bench)
- Workers are warmed up for every workload and run every test T times for S seconds. In the end, the result with the best aggregate throughput across all the workers is selected.  

## How can I run it?

1. Clone the repository: `git clone git@github.com:servicetitan/Stl.Fusion.Samples.git`
2. Run `dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj`

## Command line arguments

You can use `Run-RpcBenchmark.cmd <options>` or `dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj -- <options>` to run the benchmark.

Where `<options>` are:
- `server [url]` - starts test server @ the specified URL; the default one is [https://localhost:22444/]()
- `client [url] [client options]` - connect to test server @ the specified URL and run tests 
- `test [client options]` - start both the server and the client in the same process and runs tests.  

Key client options:
- `-cc <ClientConcurrency>` - the number of workers sharing a single client instance (HttpClient, gRPC Channel, SignalR client, etc.)
- `-w <WorkerCount>` - the total number of worker tasks
- `-d <TestDuration>` - the duration of each test in seconds, the default is `5`
- `-n <TryCount>` - the number of times to run each test to select the best result, the default is `4`
- `-b <Benchmarks>` - comma-separated list of tests to run, which must be a subset of `StlRpc,SignalR,StreamJsonRpc,MagicOnion,gRPC,HTTP`, the default is full set of tests
- `-wait` - wait for a key press before terminating.

The default client options are:
```
-cc 120 -w <CpuCount*300> -d 5 -n 4 -b <AllBenchmarks>
```


# Benchmark Results

Last updated: 8/16/2023.

Software:
- OS: Windows 11
- .NET: 8.0 Preview 7

Hardware:
- LAN tests:
  - Bandwidth: 1 Gbps 
  - Server: Intel Core i7 11800H (8 CPU cores = 16 virtual hyper-threaded cores) **constrained to 6 virtual cores**
  - Client: Ryzen Threadripper 3960X (24 CPU cores = 48 virtual hyper-threaded cores)
- Local tests: Ryzen Threadripper 3960X


## LAN tests

### LAN tests - high client concurrency (500)

Commands:
- Server: `Run-RpcBenchmark-Server.cmd`
- Client: `Run-RpcBenchmark-Client.cmd https://192.168.1.11:22444/ -cc 500 -w 10000`

![](./img/RpcBenchmark-LAN.gif)

```
System-wide settings:
  Thread pool settings:   48+ worker, 48+ I/O threads
  ByteSerializer.Default: MessagePack
Client settings:
  Server URL:           https://192.168.1.11:22444/
  Test plan:            5.00s warmup, 4 x 5.00s runs
  Total worker count:   10000
  Client concurrency:   500
  Client count:         20

Stl.Rpc:
  Sum      : 862.27K   1.19M   1.17M   1.19M ->   1.19M calls/s
  GetUser  : 821.86K 809.93K 807.60K 811.07K -> 821.86K calls/s
  SayHello : 482.01K 479.82K 480.23K 480.94K -> 482.01K calls/s
SignalR:
  Sum      : 800.22K 801.45K 794.29K 791.82K -> 801.45K calls/s
  GetUser  : 625.54K 624.14K 627.01K 621.87K -> 627.01K calls/s
  SayHello : 340.64K 345.35K 342.72K 332.22K -> 345.35K calls/s
StreamJsonRpc:
  Sum      : 173.71K 171.64K 161.88K 167.82K -> 173.71K calls/s
  GetUser  : 133.28K 132.31K 131.10K 129.99K -> 133.28K calls/s
  SayHello :  57.82K  54.53K  56.34K  53.51K ->  57.82K calls/s
MagicOnion:
  Sum      : 117.05K 120.05K 119.03K 116.09K -> 120.05K calls/s
  GetUser  : 113.83K 113.36K 101.22K  94.55K -> 113.83K calls/s
  SayHello :  91.09K  88.23K  90.37K  90.13K ->  91.09K calls/s
gRPC:
  Sum      : 109.37K 104.45K 102.33K  99.85K -> 109.37K calls/s
  GetUser  : 106.62K 102.25K 102.97K 103.16K -> 106.62K calls/s
  SayHello :  99.42K  98.16K 101.81K 100.55K -> 101.81K calls/s
HTTP:
  Sum      :  76.65K  95.31K  96.05K  96.96K ->  96.96K calls/s
  GetUser  :  97.02K  95.65K  93.25K  95.62K ->  97.02K calls/s
  SayHello :  82.91K  87.30K  86.83K  87.68K ->  87.68K calls/s  
```

### LAN tests - low client concurrency (10)

Commands:
- Server: `Run-RpcBenchmark-Server.cmd`
- Client: `Run-RpcBenchmark-Client.cmd https://192.168.1.11:22444/ -cc 10 -w 10000`

```
System-wide settings:
  Thread pool settings:   48+ worker, 48+ I/O threads
  ByteSerializer.Default: MessagePack
Client settings:
  Server URL:           https://192.168.1.11:22444/
  Test plan:            5.00s warmup, 4 x 5.00s runs
  Total worker count:   10000
  Client concurrency:   10
  Client count:         1000

Stl.Rpc:
  Sum      : 542.45K 497.41K 548.56K 498.03K -> 548.56K calls/s
  GetUser  : 489.61K 461.83K 457.90K 413.73K -> 489.61K calls/s
  SayHello : 352.40K 345.78K 366.69K 327.86K -> 366.69K calls/s
SignalR:
  Sum      : 322.26K 318.24K 318.07K 326.15K -> 326.15K calls/s
  GetUser  : 359.93K 350.13K 360.51K 355.77K -> 360.51K calls/s
  SayHello : 281.83K 279.91K 288.70K 263.40K -> 288.70K calls/s
StreamJsonRpc:
  Sum      : 143.00K 136.94K 142.42K 136.68K -> 143.00K calls/s
  GetUser  : 115.00K 116.07K 116.07K 110.61K -> 116.07K calls/s
  SayHello :  57.15K  53.85K  50.96K  52.62K ->  57.15K calls/s
MagicOnion:
  Failed with HttpRequestException: The server refused the connection.
gRPC:
  Failed with HttpRequestException: The server refused the connection.
HTTP:
  Sum      :  90.51K  91.96K  95.20K  95.78K ->  95.78K calls/s
  GetUser  :  94.84K  94.09K  94.29K  95.17K ->  95.17K calls/s
  SayHello :  82.65K  85.96K  86.53K  86.38K ->  86.53K calls/s
  
```

### LAN tests - no client concurrency (1) and low number of workers (200)

Commands:
- Server: `Run-RpcBenchmark-Server.cmd`
- Client: `Run-RpcBenchmark-Client.cmd https://192.168.1.11:22444/ -cc 1 -w 200`

```
Stl.Rpc:
  Sum      : 125.10K 123.65K 124.99K 124.50K -> 125.10K calls/s
  GetUser  : 105.78K 103.09K 103.36K 103.45K -> 105.78K calls/s
  SayHello :  94.78K  94.30K  92.85K  94.26K ->  94.78K calls/s
SignalR:
  Sum      : 112.24K 112.34K 112.34K 112.20K -> 112.34K calls/s
  GetUser  : 107.77K 106.51K 107.55K 107.55K -> 107.77K calls/s
  SayHello :  89.65K  89.70K  89.49K  89.58K ->  89.70K calls/s
StreamJsonRpc:
  Sum      :  69.17K  69.62K  69.13K  69.55K ->  69.62K calls/s
  GetUser  :  59.95K  59.17K  60.06K  58.99K ->  60.06K calls/s
  SayHello :  36.99K  36.78K  36.34K  36.95K ->  36.99K calls/s
MagicOnion:
  Sum      :  46.67K  43.72K  46.57K  45.26K ->  46.67K calls/s
  GetUser  :  41.46K  46.81K  42.38K  42.05K ->  46.81K calls/s
  SayHello :  42.11K  40.16K  39.90K  41.01K ->  42.11K calls/s
gRPC:
  Sum      :  49.02K  49.17K  47.57K  35.68K ->  49.17K calls/s
  GetUser  :  45.34K  44.85K  44.69K  45.57K ->  45.57K calls/s
  SayHello :  44.93K  44.20K  42.64K  44.65K ->  44.93K calls/s
HTTP:
  Sum      : 117.98K 118.84K 119.00K 119.27K -> 119.27K calls/s
  GetUser  : 116.55K 115.95K 116.99K 115.95K -> 116.99K calls/s
  SayHello :  88.41K  89.24K  88.63K  89.26K ->  89.26K calls/s
```
     
## Local tests

### Local tests - default settings

Command: `Run-RpcBenchmark.cmd test`

![](./img/RpcBenchmark-Local.gif)

```
System-wide settings:
  Thread pool settings:   48+ worker, 48+ I/O threads
  ByteSerializer.Default: MessagePack
Starting server @ https://localhost:22444/
Client settings:
  Server URL:           https://localhost:22444/
  Test plan:            5.00s warmup, 4 x 5.00s runs
  Total worker count:   14400
  Client concurrency:   120
  Client count:         120

Stl.Rpc:
  Sum      :   2.67M   3.14M   3.30M   3.19M ->   3.30M calls/s
  GetUser  :   2.66M   2.59M   2.64M   2.65M ->   2.66M calls/s
  SayHello :   1.75M   1.74M   1.75M   1.72M ->   1.75M calls/s
SignalR:
  Sum      :   2.75M   2.73M   2.74M   2.69M ->   2.75M calls/s
  GetUser  :   2.39M   2.36M   2.32M   2.34M ->   2.39M calls/s
  SayHello :   1.22M   1.21M   1.19M   1.17M ->   1.22M calls/s
StreamJsonRpc:
  Sum      : 227.85K 224.79K 249.84K 228.25K -> 249.84K calls/s
  GetUser  : 183.93K 181.97K 182.24K 182.15K -> 183.93K calls/s
  SayHello :  58.83K  59.15K  59.04K  59.10K ->  59.15K calls/s
MagicOnion:
  Sum      : 125.88K 120.66K 119.31K 122.00K -> 125.88K calls/s
  GetUser  : 128.26K 129.67K 125.56K 127.15K -> 129.67K calls/s
  SayHello : 125.15K 125.76K 123.67K 119.70K -> 125.76K calls/s
gRPC:
  Sum      : 128.28K 127.80K 121.23K 122.44K -> 128.28K calls/s
  GetUser  : 126.51K 127.30K 126.23K 128.52K -> 128.52K calls/s
  SayHello : 124.50K 124.58K 125.11K 123.18K -> 125.11K calls/s
HTTP:
  Sum      : 146.10K 150.21K 147.05K 147.35K -> 150.21K calls/s
  GetUser  : 148.69K 143.76K 151.10K 143.60K -> 151.10K calls/s
  SayHello : 136.11K 137.46K 134.38K 135.96K -> 137.46K calls/s
```

### Local tests - best settings for gRPC and MagicOnion

Command: `Run-RpcBenchmark.cmd test -cc 1000 -b grpc,mo`

```
System-wide settings:
  Thread pool settings:   48+ worker, 48+ I/O threads
  ByteSerializer.Default: MessagePack
Starting server @ https://localhost:22444/
Client settings:
  Server URL:           https://localhost:22444/
  Test plan:            5.00s warmup, 4 x 5.00s runs
  Total worker count:   14400
  Client concurrency:   1000
  Client count:         15

gRPC:
  Sum      : 105.63K 174.80K 187.04K 186.30K -> 187.04K calls/s
  GetUser  : 185.31K 183.92K 183.93K 184.95K -> 185.31K calls/s
  SayHello : 180.07K 175.79K 181.53K 177.62K -> 181.53K calls/s
MagicOnion:
  Sum      : 180.95K 179.12K 178.14K 181.35K -> 181.35K calls/s
  GetUser  : 175.71K 173.05K 173.18K 172.31K -> 175.71K calls/s
  SayHello : 167.32K 165.32K 169.15K 169.25K -> 169.25K calls/s
```

## Local tests + server constrained to 6 cores

Use:
- `Run-RpcBenchmark-Server.cmd <CoreCount>` to start the server **pinned to the first N CPU cores** (the default is 6)
- `Run-RpcBenchmark-Client.cmd [url] [client options]` to run the client. It's the same command as  `dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj -- client [url] [client options]` 

### 6-core server, best settings for Stl.Rpc & SignalR

Commands:
- `Run-RpcBenchmark-Server.cmd`
- `Run-RpcBenchmark-Client.cmd -wait -cc 100 -w 10000`

```
System-wide settings:
  Thread pool settings:   48+ worker, 48+ I/O threads
  ByteSerializer.Default: MessagePack
Client settings:
  Server URL:           https://localhost:22444/
  Test plan:            5.00s warmup, 4 x 5.00s runs
  Total worker count:   10000
  Client concurrency:   100
  Client count:         100

Stl.Rpc:
  Sum      :   1.04M   1.04M   1.05M   1.05M ->   1.05M calls/s
  GetUser  : 753.31K 760.69K 761.49K 772.16K -> 772.16K calls/s
  SayHello : 499.23K 501.05K 499.70K 499.56K -> 501.05K calls/s
SignalR:
  Sum      : 851.24K 857.56K 858.49K 847.26K -> 858.49K calls/s
  GetUser  : 713.97K 697.03K 698.62K 708.12K -> 713.97K calls/s
  SayHello : 313.98K 314.85K 308.84K 313.44K -> 314.85K calls/s
StreamJsonRpc:
  Sum      : 122.80K 122.54K 121.67K 124.02K -> 124.02K calls/s
  GetUser  :  93.07K  92.32K  93.97K  92.80K ->  93.97K calls/s
  SayHello :  43.37K  42.34K  43.19K  43.38K ->  43.38K calls/s
MagicOnion:
  Sum      :  74.80K  84.16K  84.04K  81.14K ->  84.16K calls/s
  GetUser  :  69.71K  79.68K  78.54K  80.98K ->  80.98K calls/s
  SayHello :  74.24K  77.60K  74.22K  76.27K ->  77.60K calls/s
gRPC:
  Sum      :  86.36K  57.64K  76.64K  77.06K ->  86.36K calls/s
  GetUser  :  86.35K  92.07K  86.61K  92.18K ->  92.18K calls/s
  SayHello :  89.01K  82.55K  84.90K  81.27K ->  89.01K calls/s
HTTP:
  Sum      :  75.53K  72.44K  75.19K  75.30K ->  75.53K calls/s
  GetUser  :  71.68K  72.56K  72.85K  70.57K ->  72.85K calls/s
  SayHello :  60.87K  58.25K  60.94K  59.99K ->  60.94K calls/s
```

### 6-core server, best settings for gRPC

Commands:
- `Run-RpcBenchmark-Server.cmd`
- `Run-RpcBenchmark-Client.cmd -wait -cc 1000 -w 10000`

```
System-wide settings:
  Thread pool settings:   48+ worker, 48+ I/O threads
  ByteSerializer.Default: MessagePack
Client settings:
  Server URL:           https://localhost:22444/
  Test plan:            5.00s warmup, 4 x 5.00s runs
  Total worker count:   10000
  Client concurrency:   1000
  Client count:         10

Stl.Rpc:
  Sum      : 959.11K 912.14K 910.62K 888.58K -> 959.11K calls/s
  GetUser  : 624.38K 615.48K 623.40K 631.55K -> 631.55K calls/s
  SayHello : 409.44K 423.51K 414.74K 421.74K -> 423.51K calls/s
SignalR:
  Sum      : 843.17K 847.50K 844.43K 843.37K -> 847.50K calls/s
  GetUser  : 646.71K 642.96K 656.32K 637.99K -> 656.32K calls/s
  SayHello : 252.49K 271.26K 260.31K 268.19K -> 271.26K calls/s
StreamJsonRpc:
  Sum      : 115.51K 117.17K 115.91K 116.46K -> 117.17K calls/s
  GetUser  :  85.15K  87.24K  87.65K  85.81K ->  87.65K calls/s
  SayHello :  48.47K  49.09K  47.96K  49.99K ->  49.99K calls/s
MagicOnion:
  Sum      :  88.01K  85.13K  87.55K  88.55K ->  88.55K calls/s
  GetUser  :  84.86K  86.22K  83.73K  84.63K ->  86.22K calls/s
  SayHello :  79.11K  79.12K  77.69K  79.11K ->  79.12K calls/s
gRPC:
  Sum      :  91.05K  86.54K  91.77K  91.46K ->  91.77K calls/s
  GetUser  :  89.83K  88.54K  88.19K  90.39K ->  90.39K calls/s
  SayHello :  86.63K  87.43K  83.50K  85.47K ->  87.43K calls/s
HTTP:
  Sum      :  75.60K  76.56K  76.91K  76.18K ->  76.91K calls/s
  GetUser  :  74.40K  74.27K  74.48K  74.57K ->  74.57K calls/s
  SayHello :  60.99K  58.05K  61.97K  59.36K ->  61.97K calls/s
```
