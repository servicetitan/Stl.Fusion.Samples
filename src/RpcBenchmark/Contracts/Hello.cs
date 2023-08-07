using System.Runtime.Serialization;
using MemoryPack;

namespace Samples.RpcBenchmark;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial class Hello
{
    public static readonly Hello ExamplePayload = new() {
        // The data is taken from:
        // - https://github.com/LesnyRumcajs/grpc_bench/blob/master/scenarios/complex_proto/payload
        Name = "a name",
        Double = 4.55332,
        Float = 232.3f,
        Bool = true,
        Int32 = 32,
        Int64 = 444325235223L,
        ChoiceString = "ofcouse",
        Pets = new Pet[] {
            new() { Name = "Bof the dog", Color = Color.Blue },
            new() { Name = "Kim the cat", Color = Color.Red },
        }
    };

    [DataMember, MemoryPackOrder(0)] public string Name { get; set; } = null!;
    [DataMember, MemoryPackOrder(1)] public double Double { get; set; }
    [DataMember, MemoryPackOrder(2)] public float Float { get; set; }
    [DataMember, MemoryPackOrder(3)] public bool Bool { get; set; }
    [DataMember, MemoryPackOrder(4)] public int Int32 { get; set; }
    [DataMember, MemoryPackOrder(5)] public long Int64 { get; set; }
    [DataMember, MemoryPackOrder(6)] public string? ChoiceString { get; set; }
    [DataMember, MemoryPackOrder(7)] public bool? ChoiceBool { get; set; }
    [DataMember, MemoryPackOrder(8)] public Pet[] Pets { get; set; } = null!;
}
