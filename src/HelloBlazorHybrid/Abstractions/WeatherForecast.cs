using System.Runtime.Serialization;
using MemoryPack;

namespace Samples.HelloBlazorHybrid.Abstractions;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public partial class WeatherForecast
{
    [DataMember, MemoryPackOrder(0)] public DateTime Date { get; set; }
    [DataMember, MemoryPackOrder(1)] public int TemperatureC { get; set; }
    [DataMember, MemoryPackOrder(2)] public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    [DataMember, MemoryPackOrder(3)] public string Summary { get; set; } = "";
}
