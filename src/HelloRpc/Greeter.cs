using System.Runtime.Serialization;
using MemoryPack;
using Stl.Rpc;
using Stl.Rpc.Infrastructure;

namespace Samples.HelloRpc;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

public interface IGreeter : IRpcService
{
    Task<Message> SayHello(string name, CancellationToken cancellationToken = default);
}

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public partial record Message(
    [property: DataMember(Order = 0), MemoryPackOrder(0)] string Text
    );

public class Greeter(IClientNotifier clientNotifier) : IGreeter
{
    private readonly Dictionary<RpcPeer, Task> _notificationTasks = new();

    public async Task<Message> SayHello(string name, CancellationToken cancellationToken = default)
    {
        StartClientNotifications(RpcInboundContext.Current!.Peer);
        return new Message($"Hello, {name}!");
    }

    private void StartClientNotifications(RpcPeer peer)
    {
        lock (_notificationTasks) {
            if (_notificationTasks.ContainsKey(peer))
                return;

            _notificationTasks.Add(peer, Task.Run(async () => {
                var cancellationToken = peer.StopToken;
                try {
                    while (true) {
                        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                        await clientNotifier.Notify(peer.Ref.Key, DateTime.Now, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally {
                    lock (_notificationTasks) {
                        _notificationTasks.Remove(peer);
                    }
                }
            }));


        }
    }
}
