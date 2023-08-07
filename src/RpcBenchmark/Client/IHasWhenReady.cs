namespace Samples.RpcBenchmark.Client;

public interface IHasWhenReady
{
    Task WhenReady { get; }
}
