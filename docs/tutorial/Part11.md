# Part 11: Fusion Authentication

Fusion has its own authentication system based on 

## Fusion Session

One of the important elements in this authentication system is Fusion's own session. 
A session is essentialy a string value, that is stored in http only cookie. if the client sends this cookie with a request then we use the session specified there
if not we create a new session and store it inside a cookie. To use this Fusion session we need to call `UseFusionSession` inside the `Configure` method of the Startup class.
This will add the `SessionMiddleware` to the request pipeline. The actual class contains a bit more logic, but the important parts for now are the following:

``` cs --editable false
public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        var cookies = httpContext.Request.Cookies;
        var cookieName = Cookie.Name ?? "";
        cookies.TryGetValue(cookieName, out var sessionId);
        var session = string.IsNullOrEmpty(sessionId) ? null : new Session(sessionId);

        if (session == null) {
            session = SessionFactory.CreateSession();
            var responseCookies = httpContext.Response.Cookies;
            responseCookies.Append(cookieName, session.Id, Cookie.Build(httpContext));
        }
        SessionProvider.Session = session;
        await next(httpContext).ConfigureAwait(false);
    }
```

The Session class in itself is very simple, it stores a string Id value (inside a Symbol) and it also specifies how it should be serialized and how should we compare it to other session objects.

```cs 
[DataContract]
[JsonConverter(typeof(SessionJsonConverter))]
[Newtonsoft.Json.JsonConverter(typeof(SessionNewtonsoftJsonConverter))]
[TypeConverter(typeof(SessionTypeConverter))]
public sealed class Session : IHasId<Symbol>, IEquatable<Session>,
    IConvertibleTo<string>, IConvertibleTo<Symbol>
{
    public static Session Null { get; } = null!; // To gracefully bypass some nullability checks

    [DataMember(Order = 0)]
    public Symbol Id { get; }
    ...
}
```

# Authentication services in the backend application

We can associate authentication info with each session. Basically we store if the session is authenticated, then who the signed in user is and what claims do they have.
On the server side the following two services interact with authentication data.

- InMemoryAuthService
- DbAuthService

They implement the same interfaces, so they can be used interchangeably the only difference between them is where they store the authentication data (in memory/database).

The in-memory service is registered by default, in order to register the DbAuthService in the DI Container we need to call the `AddAuthentication` method in a similar way
to the following code snippet. The Operations Framework is also needed for this service, you can read more about that topic tutorial Part 10.

```cs
services.AddDbContextServices<FusionDbContext>(dbContext =>
            {
                dbContext.AddOperations((_, o) => {
                    o.UnconditionalWakeUpPeriod = TimeSpan.FromSeconds(1);
                });

                dbContext.AddAuthentication<DbSessionInfo<long>, DbUser<long>, long>();
            });
```
Our DbContext needs to contain DbSets for the classes provided here as type parameters.
The `DbSessionInfo` and `DbUser` classes are very simple entities provided by Fusion for storing authentication data.

```cs
public class DbSessionInfo<TDbUserId> : IHasId<string>, IHasVersion<long>
    {
        [Key]
        [StringLength(32)]
        public string Id { get; set; }
        [ConcurrencyCheck]
        public long Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastSeenAt { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public string AuthenticatedIdentity { get; set; }
        public TDbUserId UserId { get; set; }
        public bool IsSignOutForced { get; set; }
        public string OptionsJson { get; set; }
    }
```

`DbSessionInfo` stores our sessions, and these sessions (if authenticated) can be associated with a `DbUser`




(

## Using session in Compute Services for authorization

## Using authentication services in a Blazor client

- How can I access session at client side?
- 
- How can I access authentication state/authenticated user associated with session from client side?


## Synchronizing Fusion and ASP.NET Core authentication states

_Host.cshtml
Server side rendering

await Task.Run(() => ServerAuthHelper.UpdateAuthState(HttpContext));





## Forcing sign out

## Creating your own registration/login system
