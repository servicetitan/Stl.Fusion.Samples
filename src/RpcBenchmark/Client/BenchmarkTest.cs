namespace Samples.RpcBenchmark.Client;

public delegate Task<long> BenchmarkTest(BenchmarkWorker worker, Task<CpuTimestamp> whenReady);
