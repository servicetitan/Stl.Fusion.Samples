using Google.Protobuf.WellKnownTypes;

namespace Samples.RpcBenchmark;

public static  class Examples
{
    public static readonly Hello Hello;
    public static readonly HelloRequest HelloRequest;
    public static readonly User User;

    public static readonly GrpcHello GrpcHello;
    public static readonly GrpcHelloRequest GrpcHelloRequest;
    public static readonly GrpcUser GrpcUser;

    static Examples()
    {
        Hello = new() {
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
        HelloRequest = new HelloRequest() { Request = Hello };
        User = new() {
            Id = 1,
            Version = 2,
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now,
            Name = "Bob",
        };
        GrpcHello = new() {
            // The data is taken from:
            // - https://github.com/LesnyRumcajs/grpc_bench/blob/master/scenarios/complex_proto/payload
            Name = "a name",
            Double = 4.55332,
            Float = 232.3f,
            Bool = true,
            Int32 = 32,
            Int64 = 444325235223L,
            ChoiceString = "ofcouse",
        };
        GrpcHello.Pets.Add(new GrpcHello.Types.GrpcPet() {
            Name = "Bof the dog",
            Color = GrpcHello.Types.GrpcPet.Types.GrpcColor.Blue,
        });
        GrpcHello.Pets.Add(new GrpcHello.Types.GrpcPet() {
            Name = "Kim the cat",
            Color = GrpcHello.Types.GrpcPet.Types.GrpcColor.Red,
        });
        GrpcHelloRequest = new GrpcHelloRequest() { Request = GrpcHello };
        GrpcUser = new() {
            Id = 1,
            Version = 2,
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
            ModifiedAt = Timestamp.FromDateTime(DateTime.UtcNow),
            Name = "Bob",
        };
    }

}
