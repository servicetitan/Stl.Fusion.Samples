using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.Abstractions
{
    public record ComposedValue
    {
        public string Parameter { get; init; } = "";
        public double Uptime { get; init; }
        public double? Sum { get; init; }
        public string LastChatMessage { get; init; } = "";
        public User? User { get; init; }
        public long ActiveUserCount { get; init; }
    }

    public interface IComposerService
    {
        [ComputeMethod(KeepAliveTime = 1)]
        Task<ComposedValue> GetComposedValue(string parameter,
            Session session, CancellationToken cancellationToken = default);
    }

    public interface ILocalComposerService : IComposerService { }
}
