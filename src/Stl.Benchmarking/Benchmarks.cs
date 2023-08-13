namespace Stl.Benchmarking;

public static class Benchmarks
{
    private const long CancellationTokenSourceRenewMask = 127;

    public static Task<double> CallFrequency(
        int workerCount,
        double duration,
        CancellationToken cancellationToken,
        Func<int, Func<CancellationToken, Task>> workerOperationFactory,
        Func<int, Task>? workerReadyFactory = null)
        => CallFrequency(
            Enumerable.Range(0, workerCount).ToArray(),
            duration,
            cancellationToken,
            workerOperationFactory,
            workerReadyFactory);

    public static async Task<double> CallFrequency<TWorker>(
        TWorker[] workers,
        double duration,
        CancellationToken cancellationToken,
        Func<TWorker, Func<CancellationToken, Task>> workerOperationFactory,
        Func<TWorker, Task>? workerReadyFactory = null,
        Func<TWorker, bool>? backgroundWorkerPredicate = null)
    {
        var endsAtSource = TaskCompletionSourceExt.New<CpuTimestamp>();
        var tasks = new (Task<long> Task, bool IsBackground)[workers.Length];
        for (var i = 0; i < workers.Length; i++) {
            var worker = workers[i];
            var task = CallCount(endsAtSource.Task, cancellationToken, workerOperationFactory.Invoke(worker));
            var isBackground = backgroundWorkerPredicate != null && backgroundWorkerPredicate.Invoke(worker);
            tasks[i] = (task, isBackground);
            if (workerReadyFactory != null)
                await workerReadyFactory.Invoke(worker).ConfigureAwait(false);
        }
        endsAtSource.SetResult(CpuTimestamp.Now + TimeSpan.FromSeconds(duration));

        var sum = 0L;
        foreach (var (task, isBackground) in tasks) {
            var result = await task.ConfigureAwait(false);
            if (!isBackground)
                sum += result;
        }
        return sum / duration;
    }

    public static async Task<long> CallCount(
        Task<CpuTimestamp> endsAtTask,
        CancellationToken cancellationToken,
        Func<CancellationToken, Task> operation)
    {
        CancellationTokenSource? cts = null;
        var count = 0L;
        var endsAt = await endsAtTask.ConfigureAwait(false);
        var now = CpuTimestamp.Now;
        while (now < endsAt) {
            if ((count & CancellationTokenSourceRenewMask) == 0) {
                cts?.Dispose();
                cts = cancellationToken.CreateLinkedTokenSource();
            }
            await operation.Invoke(cts!.Token).ConfigureAwait(false);
            count++;
            now = CpuTimestamp.Now;
        }
        cts?.Dispose();
        return count;
    }
}
