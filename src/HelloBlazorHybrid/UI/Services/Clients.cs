using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using HelloBlazorHybrid.Abstractions;

namespace HelloBlazorHybrid.UI.Services
{
    [BasePath("counter")]
    public interface ICounterClient
    {
        [Post("increment")]
        Task Increment(CancellationToken cancellationToken = default);

        [Get("get")]
        Task<int> Get(CancellationToken cancellationToken = default);
    }

    [BasePath("weatherForecast")]
    public interface IWeatherForecastClient
    {
        [Get("getForecast")]
        Task<WeatherForecast[]> GetForecastAsync(DateTime startDate, CancellationToken cancellationToken = default);
    }

    [BasePath("chat")]
    public interface IChatClient
    {
        [Post("postMessage")]
        Task PostMessageAsync([Body] IChatService.PostCommand command, CancellationToken cancellationToken = default);
        
        [Get("getMessageCount")]
        Task<int> GetMessageCountAsync();

        [Get("getMessages")]
        Task<(DateTime Time, string Name, string Message)[]> GetMessagesAsync(int count,
            CancellationToken cancellationToken = default);

        [Get("getAnyTail")]
        Task<Unit> GetAnyTailAsync();
    }
}