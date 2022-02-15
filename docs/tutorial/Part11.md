# Part 11: Fusion Authentication

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

## Synchronizing Fusion and ASP.NET Core authentication states

We can create a Host.cshtmL file in the server project, the code inside here will run on each page load, so this is an ideal place to synchronize authentication states.
The synchronization is done by the `UpdateAuthState` method. This method updates the Fusion authentication state (in memory/database) to match what's currently inside the HttpContext, in the background it calls `IAuthBackend.SignIn()` and `IAuthBackend.SignOut` depending on the current authentication state.

The following code snippet shows a typical implementation of this scenario

``` cs
@page "/"
@inject ServerAuthHelper _serverAuthHelper
@inject BlazorCircuitContext _blazorCircuitContext
@{
    await _serverAuthHelper.UpdateAuthState(HttpContext);
    var authSchemas = await _serverAuthHelper.GetSchemas(HttpContext);
    var sessionId = _serverAuthHelper.Session.Id.Value;
    Layout = null;
}

<!DOCTYPE html>
<html lang="en">
<head>
    <script>
        window.FusionAuth.schemas = "@authSchemas";
        window.FusionAuth.sessionId = "@sessionId";
    </script>
...
```

## Using authentication services in a Blazor client

Because the client-side Replica Services have the same interface as their server-side Compute Service counterparts, the client needs to pass the session as a parameter for methods that require it. This server doesn't really require this, because it can (and does) read the session from the cookie sent with the request, but it is neccessary for client side caching, so that we can store separate entries for different session values.

However the session is stored in a http-only cookie, so the client can't read its value directly, but as seen in the previous code snippet (Host.cshtml) we pass the value of the session to the client using the `window.FusionAuth.sessionId` variable that the client can read. After this we can inject our session anywhere in the Blazor client, we just need to write `@inject Session Session` in the beggining of our components. Then we can use the `IAuth` service on the client to access the current user, and our authentication state in the same way as we do on the server.

``` cs
@page "/myOrders
@inherits ComputedStateComponent<List<OrderHeaderDto>>
@inject IOrderService OrderService
@inject Session _session
@inject IAuth _auth

protected override async Task<List<OrderHeaderDto>> ComputeState(CancellationToken cancellationToken)
    {
        var user = await AuthService.GetUser(Session, cancellationToken);
        var sessionInfo = await AuthService.GetSessionInfo(Session, cancellationToken);

        if(user.IsAuthenticated)
        {
            if(user.Claims.ContainsKey("required-claim"))
            {
                return await OrderService.GetMyOrders(Session);
            }
        }
    }
```

Another important thing concerning sessions is that even if someone managed to steal or correctly guess your session id and send it to the server it still wouldn't matter because the server can read you actual session from your cookie and use that instead. In the following example the server application uses the session from the cookie, instead of the one provided by the client.

``` cs
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : ControllerBase, IOrderService
    {
        private readonly IOrderService _orderService;
        private readonly ISessionResolver _sessionResolver;

        public OrderController(IOrderService orderService, ISessionResolver sessionResolver)
        {
            _orderService = orderService;
            _sessionResolver = sessionResolver;
        }

        [HttpGet, Publish]
        public async Task<List<OrderHeaderDto>> GetMyOrders([FromQuery] Session session, CancellationToken cancellationToken = default)
        {
            return await _orderService.GetMyOrders(_sessionResolver.Session, cancellationToken);
        }
    }
```

## Forcing sign out

You can force sign-out for a specific session. This feauture is implemented in the `SessionMiddleware` class, that executes with every request. Basically we check the state of our session (stored in-memory/in database), and if we requested a forced sign out, then we call a registered handler that signs out the user, deletes the cookie that the session was stored in and optionally redirects them to somewhere else in the application. The relevant parts of the `SessionMiddleware` class are the following:

``` cs
public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        var cancellationToken = httpContext.RequestAborted;
        var cookies = httpContext.Request.Cookies;
        var cookieName = Cookie.Name ?? "";
        cookies.TryGetValue(cookieName, out var sessionId);
        var session = string.IsNullOrEmpty(sessionId) ? null : new Session(sessionId);
        if (session != null) {
            if (Auth != null) {
                var isSignOutForced = await Auth.IsSignOutForced(session, cancellationToken).ConfigureAwait(false);
                if (isSignOutForced) {
                    if (await ForcedSignOutHandler(this, httpContext).ConfigureAwait(false)) {
                        var responseCookies = httpContext.Response.Cookies;
                        responseCookies.Delete(cookieName);
                        return;
                    }
                    session = null;
                }
            }
        }
        // ...
    }
```

You can create your own `ForcedSignOutHandler` or use the default implementation provided by Fusion.

``` cs
public static async Task<bool> DefaultForcedSignOutHandler(SessionMiddleware self, HttpContext httpContext)
        {
            await httpContext.SignOutAsync().ConfigureAwait(false);
            var url = httpContext.Request.GetEncodedPathAndQuery();
            httpContext.Response.Redirect(url);
            // true:  reload: redirect w/o invoking the next middleware
            // false: proceed normally, i.e. invoke the next middleware
            return true;
        }
```

## Creating your own registration/login system with ASP.NET Core Identity

You can use Fusion's authentication system on top of ASP.NET Core Identity. If you follow this approach then you will need to synchronize the authentication state between the two frameworks, the following code shows a very a basic implementation of this.

Our SignIn methods needs to receive the username and password of the user that wants to sign in, and also the current Fusion session.
Then we can check if the provided data is valid in the example we basically check if
    - The user exists
    - If the user is already signed in or not in Fusions authentication state
    - Whether the password/email pair is correct

``` cs
public async Task SignIn(Session session, EmailPasswordDto signInDto, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(signInDto.Email);
            if (user is ApplicationUser)
            {
                var sessionInfo = await _authService.GetSessionInfo(session, cancellationToken);
                if (sessionInfo.IsAuthenticated)
                    throw new Exception("You are already signed in");

                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, signInDto.Password, lockoutOnFailure: false);
                
            }
        }
```

If everything was correct then we can proceed with signing the user in. The basic idea here is that we store what the claims and roles of each user with the Identity framework, inside the database, and during the signin process we query these roles and claims from here using the `UserManager` service that Identity provides and we can create a `ClaimsPrincipa` from these values that we can pass to the Fusion SignIn method.

``` cs
    if (signInResult.Succeeded)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
        var principal = new ClaimsPrincipal(identity);

        var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var userAgent = _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("User-Agent", out var userAgentValues)
                        ? userAgentValues.FirstOrDefault() ?? ""
                        : "";

        var mustUpdateSessionInfo =
            !StringComparer.Ordinal.Equals(sessionInfo.IPAddress, ipAddress)
            || !StringComparer.Ordinal.Equals(sessionInfo.UserAgent, userAgent);
        if (mustUpdateSessionInfo)
        {
            var setupSessionCommand = new SetupSessionCommand(session, ipAddress, userAgent);
            await _authService.SetupSession(setupSessionCommand, cancellationToken);
        }

        var fusionUser = new User(session.Id);
        var (newUser, authenticatedIdentity) = CreateFusionUser(fusionUser, principal, CookieAuthenticationDefaults.AuthenticationScheme);
        var signInCommand = new SignInCommand(session, newUser, authenticatedIdentity);
        signInCommand.IsServerSide = true;
        await _authService.SignIn(signInCommand, cancellationToken);
     }
     
     protected virtual (User User, UserIdentity AuthenticatedIdentity) CreateFusionUser(User user, ClaimsPrincipal httpUser, string schema)
        {
            var httpUserIdentityName = httpUser.Identity?.Name ?? "";
            var claims = httpUser.Claims.ToImmutableDictionary(c => c.Type, c => c.Value);
            var id = FirstClaimOrDefault(claims, IdClaimKeys) ?? httpUserIdentityName;
            var name = FirstClaimOrDefault(claims, NameClaimKeys) ?? httpUserIdentityName;
            var identity = new UserIdentity(schema, id);
            var identities = ImmutableDictionary<UserIdentity, string>.Empty.Add(identity, "");

            user = new User("", name)
            {
                Claims = claims,
                Identities = identities
            };
            return (user, identity);
        }

        protected static string? FirstClaimOrDefault(IReadOnlyDictionary<string, string> claims, string[] keys)
        {
            foreach (var key in keys)
                if (claims.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
                    return value;
            return null;
        }
```

After calling Fusion's `_authService.SignIn()` the authentication state will be stored inside Fusion's storage and a cookie will also be created, so we can proceed as usual. One thing we need to be careful with is if we edit the roles/claims of a certain user inside Identity, we will need to invalidate this inside Fusion's storage or maybe even force the user to sign out, in order to keep the two frameworks in sync. To update the authentication state inside Fusion we can simply call `_authService.SignIn` with a newly constructed `ClaimsPrincipal` object containing the updated roles/claims.

