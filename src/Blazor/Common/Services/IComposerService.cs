using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stl.Fusion;
using Stl.Fusion.Authentication;

namespace Samples.Blazor.Common.Services
{
    public class ComposedValue
    {
        public string Parameter { get; } = "";
        public DateTime Time { get; }
        public string LastChatMessage { get; } = "";
        public AuthUser User { get; } = new AuthUser("");
        public long ActiveUserCount { get; }

        public ComposedValue() { }
        [JsonConstructor]
        public ComposedValue(string parameter, DateTime time, string lastChatMessage, AuthUser user, long activeUserCount)
        {
            Parameter = parameter;
            Time = time;
            LastChatMessage = lastChatMessage;
            User = user;
            ActiveUserCount = activeUserCount;
        }
    }

    public interface IComposerService
    {
        [ComputeMethod(KeepAliveTime = 1)]
        Task<ComposedValue> GetComposedValueAsync(string parameter, AuthContext? context, CancellationToken cancellationToken = default);
    }

    public interface ILocalComposerService : IComposerService { }
}
