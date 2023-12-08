using System.Runtime.Serialization;
using MemoryPack;
using Stl.Rpc;

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

public class Greeter : IGreeter
{
    public async Task<Message> SayHello(string name, CancellationToken cancellationToken = default)
        => new($"Hello, {name}!");
}
