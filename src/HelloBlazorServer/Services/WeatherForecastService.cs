using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Samples.HelloBlazorServer.Models;
using Stl.Fusion;

namespace Samples.HelloBlazorServer.Services
{
    [ComputeService]
    public class WeatherForecastService
    {
        private static readonly string[] Summaries = {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [ComputeMethod(AutoInvalidateTime = 1)]
        public virtual Task<WeatherForecast[]> GetForecastAsync(
            DateTime startDate, CancellationToken cancellationToken = default)
        {
            var rng = new Random();
            return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray());
        }
    }
}
