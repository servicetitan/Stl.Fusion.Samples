using Samples.HelloBlazorServer.Models;

namespace Samples.HelloBlazorServer.Services;

public class WeatherForecastService
{
    private static readonly string[] Summaries = {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [ComputeMethod(AutoInvalidationDelay = 1)]
    public virtual Task<WeatherForecast[]> GetForecast(
        Moment startDate, CancellationToken cancellationToken = default)
    {
        var rng = new Random();
        return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = startDate.ToDateTime().AddDays(index),
            TemperatureC = rng.Next(-20, 55),
            Summary = Summaries[rng.Next(Summaries.Length)]
        }).ToArray());
    }
}
