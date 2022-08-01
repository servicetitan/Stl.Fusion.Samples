namespace Samples.HelloBlazorHybrid.Abstractions;

public interface IWeatherForecastService
{
    [ComputeMethod]
    Task<WeatherForecast[]> GetForecast(Moment startDate, CancellationToken cancellationToken = default);
}
