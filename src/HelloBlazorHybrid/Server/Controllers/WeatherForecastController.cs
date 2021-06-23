using System;
using System.Threading;
using System.Threading.Tasks;
using HelloBlazorHybrid.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;

namespace HelloBlazorHybrid.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController, JsonifyErrors]
    public class WeatherForecastController : ControllerBase, IWeatherForecastService
    {
        private readonly IWeatherForecastService _forecast;
        
        public WeatherForecastController(IWeatherForecastService forecast) => _forecast = forecast;

        [HttpGet, Publish]
        public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate,
            CancellationToken cancellationToken = default)
            => _forecast.GetForecastAsync(startDate, cancellationToken);
    }
}