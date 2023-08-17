# RpcBenchmark

To run the benchmark:

1. Clone the repository: `git clone git@github.com:servicetitan/Stl.Fusion.Samples.git`
2. Run `dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj`


## Arguments

You can use `Run-RpcBenchmark.cmd <options>` or `dotnet run -c Release --project src/RpcBenchmark/RpcBenchmark.csproj -- <options>` to run the benchmark.

Where `<options>` are:
- `server [url]` - starts test server @ the specified URL; the default one is https://localhost:22444/
- `client [url] [client options]` - connect to test server @ the specified URL and run tests 
- `test [client options]` - start both the server and the client in the same process and runs tests.  

Key client options:
- `-cc <ClientConcurrency>` - the number of workers sharing a single client instance (HttpClient, gRPC Channel, SignalR client, etc.)
- `-w <WorkerCount>` - the total number of worker tasks
- `-d <TestDuration>` - the duration of each test in seconds, the default is `5`
- `-n <TryCount>` - the number of times to run each test to select the best result, the default is `4`
- `-b <Benchmarks>` - comma-separated list of tests to run, which must be a subset of `StlRpc,SignalR,StreamJsonRpc,MagicOnion,gRPC,HTTP`, the default is full set of tests
- `-wait` - wait for a key press before terminating.

The defaults for client options are:
```
-cc 120 -w <CpuCount*300> -d 5 -n 4 -b <AllBenchmarks>
```
         

## Results on 8/16/2023

OS: Windows 11
.NET: 8.0 Preview 7 
Hardware:
- LAN tests:
  - Bandwidth: 1 Gbps 
  - Server: Intel Core i7 11800H (8 CPU cores = 16 virtual hyper-threaded cores) **constrained to 6 virtual cores**
  - Client: Ryzen Threadripper 3960X (24 CPU cores = 48 virtual hyper-threaded cores)
- Local tests: Ryzen Threadripper 3960X

### LAN tests

### Best settings for Stl.Rpc + SignalR

Commands:
- Server: `Run-RpcBenchmark-Server.cmd`
- Client: `Run-RpcBenchmark-Client.cmd https://192.168.1.11:22444/ -cc 100 -w 10000`

```
System-wide settings:
  Thread pool settings:   48+ worker, 48+ I/O threads
  ByteSerializer.Default: MessagePack
Client settings:
  Server URL:           https://192.168.1.11:22444/
  Test plan:            5.00s warmup, 4 x 5.00s runs
  Total worker count:   10000
  Client concurrency:   100
  Client count:         100

Stl.Rpc:
  Sum      :   1.01M 985.48K 985.28K 996.63K ->   1.01M calls/s
  GetUser  : 865.59K 865.36K 799.24K 798.86K -> 865.59K calls/s
  SayHello : 478.00K 477.89K 477.45K 478.50K -> 478.50K calls/s
SignalR:
  Sum      : 640.09K 590.20K 669.51K 642.79K -> 669.51K calls/s
  GetUser  : 616.87K 624.90K 633.10K 632.79K -> 633.10K calls/s
  SayHello : 336.57K 333.72K 336.09K 337.83K -> 337.83K calls/s
StreamJsonRpc:
  Sum      : 174.99K 189.00K 190.19K 187.40K -> 190.19K calls/s
  GetUser  : 139.18K 139.01K 138.86K 138.72K -> 139.18K calls/s
  SayHello :  57.70K  56.05K  55.48K  56.57K ->  57.70K calls/s
```

### Run with default settings

Options: none (it's the same as just `test`)

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

## Best settings for gRPC and MagicOnion

Options: `test -cc 1000 -b grpc,mo`

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

## Runs with server constrained to N cores

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
