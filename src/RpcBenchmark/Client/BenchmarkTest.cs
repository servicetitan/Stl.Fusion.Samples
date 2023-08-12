namespace Samples.RpcBenchmark.Client;

public delegate Task<BenchmarkResult> BenchmarkTest(BenchmarkWorker worker, Task<CpuTimestamp> whenReady);
