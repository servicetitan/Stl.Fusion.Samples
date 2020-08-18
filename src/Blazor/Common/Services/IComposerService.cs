using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stl.Fusion;

namespace Samples.Blazor.Common.Services
{
    public class ComposedValue
    {
        public string Parameter { get; } = "";
        public DateTime Time { get; }
        public string LastChatMessage { get; } = "";
        public long ActiveUserCount { get; }

        public ComposedValue() { }
        [JsonConstructor]
        public ComposedValue(string parameter, DateTime time, string lastChatMessage, long activeUserCount)
        {
            Parameter = parameter;
            Time = time;
            LastChatMessage = lastChatMessage;
            ActiveUserCount = activeUserCount;
        }
    }

    public interface IComposerService
    {
        [ComputeMethod(KeepAliveTime = 1)]
        Task<ComposedValue> GetComposedValueAsync(string parameter, CancellationToken cancellationToken = default);
    }

    public interface ILocalComposerService : IComposerService { }
}
