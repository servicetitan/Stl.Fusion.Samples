using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MemoryPack;
using Stl.Rpc;

namespace Samples.Blazor.Abstractions;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial class Screenshot
{
    public static readonly byte[] OnePixelData = Convert.FromBase64String(
        "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAUDBAQEAwUEBAQFBQUGBwwIBwcHBw8LCwkMEQ8SEhEPERETFhwXExQaFRERGCEYGh0d" +
        "Hx8fExciJCIeJBweHx7/2wBDAQUFBQcGBw4ICA4eFBEUHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4e" +
        "Hh4eHh4eHh7/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAj/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QA" +
        "FAEBAAAAAAAAAAAAAAAAAAAAAP/EABQRAQAAAAAAAAAAAAAAAAAAAAD/2gAMAwEAAhEDEQA/ALLAB//Z");

    [DataMember, MemoryPackOrder(0)] public int Width { get; } = 1;
    [DataMember, MemoryPackOrder(1)] public int Height { get; } = 1;
    [DataMember, MemoryPackOrder(2)] public byte[] Data { get; }

    public Screenshot() => Data = OnePixelData;

    [JsonConstructor, MemoryPackConstructor]
    public Screenshot(int width, int height, byte[] data)
    {
        Width = width;
        Height = height;
        Data = data;
    }
}

public interface IScreenshotService : IComputeService
{
    Task<RpcStream<Screenshot>> StreamScreenshots(int width, CancellationToken cancellationToken = default);
    [ComputeMethod(MinCacheDuration = 0.1)]
    Task<Screenshot> GetScreenshot(int width, CancellationToken cancellationToken = default);
}
