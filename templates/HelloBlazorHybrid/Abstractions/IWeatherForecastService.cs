using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace HelloBlazorHybrid.Abstractions
{
    public interface IWeatherForecastService
    {
        [ComputeMethod]
        Task<WeatherForecast[]> GetForecastAsync(DateTime startDate, CancellationToken cancellationToken = default);
    }
}