using RestEase;

namespace Samples.HelloBlazorHybrid.Abstractions;

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
    Task<WeatherForecast[]> GetForecast(Moment startDate, CancellationToken cancellationToken = default);
}

[BasePath("chat")]
public interface IChatClientDef
{
    [Post("postMessage")]
    Task PostMessage([Body] IChatService.PostCommand command, CancellationToken cancellationToken = default);

    [Get("getMessageCount")]
    Task<int> GetMessageCount();

    [Get("getMessages")]
    Task<IChatService.Message[]> GetMessages(int count, CancellationToken cancellationToken = default);

    [Get("getAnyTail")]
    Task<Unit> GetAnyTail();
}
