using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Templates.Blazor3.Host.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet("~/signin")]
        [HttpGet("~/signin/{provider}")]
        public IActionResult SignIn(string? provider = null, string? returnUrl = null)
        {
            provider ??= GitHubAuthenticationDefaults.AuthenticationScheme;
            returnUrl ??= "/";
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, provider);
        }

        [HttpGet("~/signout")]
        [HttpPost("~/signout")]
        public IActionResult SignOut(string? returnUrl = null)
        {
            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            returnUrl ??= "/";
            return SignOut(
                new AuthenticationProperties { RedirectUri = returnUrl },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
