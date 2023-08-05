using static System.Console;

namespace Samples.HelloWorld;

public class IncrementalBuilder : IComputeService
{
    private readonly ConcurrentDictionary<string, Project> _projects = new();
    private readonly ConcurrentDictionary<string, long> _versions = new();

    [ComputeMethod]
    public virtual async Task<ProjectBuildResult> GetOrBuild(string projectId, CancellationToken cancellationToken = default)
    {
        WriteLine($"> Building: {projectId}");
        // Get project & new version of its output
        var project = _projects[projectId];
        var version = _versions.AddOrUpdate(projectId, id => 1, (id, version) => version + 1);
        // Build dependencies
        await Task.WhenAll(project.DependsOn.Select(
            // IMPORTANT: Noticed recursive GetOrBuild call below?
            // Such calls - i.e. calls made inside [ComputeMethod]-s to
            // other [ComputeMethod]-s - is all Fusion needs to know that
            // A (currently produced output) depends on B (the output of
            // whatever is called).
            // Note it's also totally fine to run such calls concurrently.
            dependencyId => GetOrBuild(dependencyId, cancellationToken)));
        // Simulate build
        await Task.Delay(100);

        var result = new ProjectBuildResult() {
            Project = project,
            Version = version,
            Artifacts = $"{projectId}.lib",
        };
        WriteLine($"< {projectId}: {result.Artifacts}, v{result.Version}");
        return result;
    }

    public Task AddOrUpdate(Project project, CancellationToken cancellationToken = default)
    {
        _projects.AddOrUpdate(project.Id, id => project, (id, _) => project);
        InvalidateGetOrBuildResult(project.Id);
        return Task.CompletedTask;
    }

    public void InvalidateGetOrBuildResult(string projectId)
    {
        // WriteLine($"Invalidating build results for: {projectId}");
        using var invalidating = Computed.Invalidate();
        // Invalidation call to [ComputeMethod] always completes synchronously, so...
        _ = GetOrBuild(projectId, default);
    }
}
