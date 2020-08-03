using System;
using System.Threading;
using System.Threading.Tasks;
using Samples.HelloBlazorServer.Services;
using Stl.Fusion.UI;

namespace Samples.HelloBlazorServer.Models
{
    public class WeatherForecastState
    {
        public WeatherForecast[] Forecasts { get; set; } = new WeatherForecast[0];

        public class Local
        {
            public DateTime StartDate { get; set; } = DateTime.Today;
        }

        [LiveStateUpdater]
        public class Updater : ILiveStateUpdater<Local, WeatherForecastState>
        {
            private WeatherForecastService WeatherForecastService { get; }

            public Updater(WeatherForecastService weatherForecastService)
                => WeatherForecastService = weatherForecastService;

            public async Task<WeatherForecastState> UpdateAsync(
                ILiveState<Local, WeatherForecastState> liveState, CancellationToken cancellationToken)
            {
                var local = liveState.Local;
                var forecasts = await WeatherForecastService.GetForecastAsync(local.StartDate, cancellationToken);
                return new WeatherForecastState() {
                    Forecasts = forecasts
                };
            }
        }
    }
}
