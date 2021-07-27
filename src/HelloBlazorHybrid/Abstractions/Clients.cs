using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using RestEase;

namespace Samples.HelloBlazorHybrid.Abstractions
{
    [BasePath("counter")]
    public interface ICounterClientDef
    {
        [Post("increment")]
        Task Increment(CancellationToken cancellationToken = default);

        [Get("get")]
        Task<int> Get(CancellationToken cancellationToken = default);
    }

    [BasePath("weatherForecast")]
    public interface IWeatherForecastClientDef
    {
        [Get("getForecast")]
        Task<WeatherForecast[]> GetForecastAsync(DateTime startDate, CancellationToken cancellationToken = default);
    }

    [BasePath("chat")]
    public interface IChatClientDef
    {
        [Post("postMessage")]
        Task PostMessageAsync([Body] IChatService.PostCommand command, CancellationToken cancellationToken = default);

        [Get("getMessageCount")]
        Task<int> GetMessageCountAsync();

        [Get("getMessages")]
        Task<IChatService.Message[]> GetMessagesAsync(int count, CancellationToken cancellationToken = default);

        [Get("getAnyTail")]
        Task<Unit> GetAnyTailAsync();
    }
}
