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
These two interfaces are `IAuth` and `IAuthBackend`. These interfaces are separate because some functions are accessable both client and server side, while there are functions
only accessable server side, for example signing in a session and associating it with a user.

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

``` cs
public class DbUser<TDbUserId> : IHasId<TDbUserId>, IHasVersion<long> where TDbUserId : notnull
    {
        public DbUser();

        [Key]
        public TDbUserId Id { get; set; }
        [ConcurrencyCheck]
        public long Version { get; set; }
        [MinLength(3)]
        public string Name { get; set; }
        public string ClaimsJson { get; set; }
        [JsonIgnore]
        [NotMapped]
        public ImmutableDictionary<string, string> Claims { get; set; }
        public List<DbUserIdentity<TDbUserId>> Identities { get; }
    }
```

## Using session in Compute Services for authorization

Our Compute Services can receive a Session object, that we can use to decide if we are authenticated or not and who the signed in user is.
We can use a `IAuthService` for this purpose in a similar way to the following code snippet

``` cs
[ComputeMethod]
public virtual async Task<List<OrderHeaderDto>> GetMyOrders(Session session, CancellationToken cancellationToken = default)
        {
            await using var dbContext = CreateDbContext();

            var sessionInfo = await _authService.GetSessionInfo(session, cancellationToken);
            if(sessionInfo.IsAuthenticated)
            {
                var user = await _authService.GetUser(session, CancellationToken.None);

                if(user.Claims.ContainsKey("read_orders"))
                {
                    ...
                }
            }
            ...
         }
```

`GetSessionInfo` and `GetUser` are also a compute methods, so they will registered a dependencies of our method, meaning that if anything changes regarding the state of our session our method will also get invalidated.


## Using authentication services in a Blazor client

- How can I access session at client side?
- How can I access authentication state/authenticated user associated with session from client side?
- What if someone steals (or is able to correctly guess) my session?


## Synchronizing Fusion and ASP.NET Core authentication states

_Host.cshtml
Server side rendering

await Task.Run(() => ServerAuthHelper.UpdateAuthState(HttpContext));

We can create a Host.cshtmL file in the server project, the code inside here will run on each page load, so this is an ideal place to synchronize authentication states.

``` cs
@page "/"
@inject ServerAuthHelper _serverAuthHelper
@inject BlazorCircuitContext _blazorCircuitContext
@{
    await _serverAuthHelper.UpdateAuthState(HttpContext);
    var authSchemas = await _serverAuthHelper.GetSchemas(HttpContext);
    var sessionId = _serverAuthHelper.Session.Id.Value;
    var isServerSideBlazor = BlazorModeController.IsServerSideBlazor(HttpContext);
    var isCloseWindowRequest = _serverAuthHelper.IsCloseWindowRequest(HttpContext, out var closeWindowFlowName);
    Layout = null;
}

<!DOCTYPE html>
<html lang="en">
<head>
    <script>
        window.FusionAuth.schemas = "@authSchemas";
        window.FusionAuth.sessionId = "@sessionId";
    </script>
```

## Forcing sign out

## Using external authentication

## Creating your own registration/login system with ASP.NET Core Identity

-Need to call `IAuthBackend.SignIn`
-Need to create cookie with authentication data
-Need to store claims from identity to fusion auth storage (

``` cs
public async Task SignIn(Session session, EmailPasswordDto signInDto, CancellationToken cancellationToken)
        {
            ...
            var fusionUser = new User(session.Id);
            var (newUser, authenticatedIdentity) = CreateFusionUser(fusionUser, principal, CookieAuthenticationDefaults.AuthenticationScheme);
            var signInCommand = new SignInCommand(session, newUser, authenticatedIdentity);
            signInCommand.IsServerSide = true;
            await _authService.SignIn(signInCommand, cancellationToken);
                
            var user = await _userManager.FindByNameAsync(signInDto.Email);
        }
```

-Need to call `IAuthBackend.SignOut`

