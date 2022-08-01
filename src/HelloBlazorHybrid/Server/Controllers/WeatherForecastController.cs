using Microsoft.AspNetCore.Mvc;
using Samples.HelloBlazorHybrid.Abstractions;
using Stl.Fusion.Server;

namespace Samples.HelloBlazorHybrid.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController, JsonifyErrors, UseDefaultSession]
public class WeatherForecastController : ControllerBase, IWeatherForecastService
{
    private readonly IWeatherForecastService _forecast;

    public WeatherForecastController(IWeatherForecastService forecast) 
        => _forecast = forecast;

    [HttpGet, Publish]
    public Task<WeatherForecast[]> GetForecast(Moment startDate, CancellationToken cancellationToken = default)
        => _forecast.GetForecast(startDate, cancellationToken);
}
