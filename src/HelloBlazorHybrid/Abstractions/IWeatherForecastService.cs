using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Samples.HelloBlazorHybrid.Abstractions
{
    public interface IWeatherForecastService
    {
        [ComputeMethod]
        Task<WeatherForecast[]> GetForecast(DateTime startDate, CancellationToken cancellationToken = default);
    }
}
