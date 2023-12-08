namespace Stl.Benchmarking;

public abstract class BenchmarkRunnerBase<TResult>
{
    public string Title { get; set; } = "(untitled)";
    public int TryCount { get; init; } = 4;
    public string Units { get; init; } = "calls/s";
    public Func<string, string> TitleFormatter { get; init; } = title => $"  {title}: ";
    public Func<TResult, string> ResultFormatter { get; init; } = result => result?.ToString() ?? "n/a";
    public IComparer<TResult> ResultComparer { get; init; } = Comparer<TResult>.Default;
    public Action<string> Writer { get; init; } = Write;
    public bool WriteNewLine { get; init; } = true;

    public async Task Run(CancellationToken cancellationToken = default)
    {
        Writer.Invoke(TitleFormatter.Invoke(Title));

        await Reset();
        await Warmup(cancellationToken);

        var bestResult = default(TResult)!;
        for (var i = 0; i < TryCount; i++) {
            await Reset();
            var result = await Benchmark(cancellationToken).ConfigureAwait(false);
            Writer.Invoke(ResultFormatter.Invoke(result) + " ");

            bestResult = i == 0 || ResultComparer.Compare(result, bestResult) > 0
                ? result
                : bestResult;
        }
        if (TryCount > 1)
            Writer.Invoke("-> " + ResultFormatter.Invoke(bestResult) + " ");
        Writer.Invoke(Units);
        if (WriteNewLine)
            Writer.Invoke(Environment.NewLine);
    }

    // Protected methods

    protected abstract Task Warmup(CancellationToken cancellationToken);
    protected abstract Task<TResult> Benchmark(CancellationToken cancellationToken);

    protected static async Task Reset()
    {
        for (var i = 0; i < 3; i++) {
            if (i != 0)
                await Task.Delay(50);
            GC.Collect();
        }
    }
}
