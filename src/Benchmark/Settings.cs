namespace Samples.Benchmark;

public static class Settings
{
    public static readonly string BaseUrl = "http://localhost:22333/";
    public static readonly string DbConnectionString =
        "Server=localhost;Database=stl_fusion_benchmark;Port=5432;User Id=postgres;Password=postgres";
    public static readonly int DbItemCount = 1000;
}
