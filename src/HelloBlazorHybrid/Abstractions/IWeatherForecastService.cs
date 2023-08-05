namespace Samples.HelloBlazorHybrid.Abstractions;

public interface IWeatherForecastService : IComputeService
{
    [ComputeMethod]
    Task<WeatherForecast[]> GetForecast(DateTime startDate, CancellationToken cancellationToken = default);
}
