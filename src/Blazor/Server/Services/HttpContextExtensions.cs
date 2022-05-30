using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Samples.Blazor.Server.Services;

public static class HttpContextExtensions
{
    public static async Task<AuthenticationScheme[]> GetExternalProviders(this HttpContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
        return (
            from scheme in await schemes.GetAllSchemesAsync()
            where !string.IsNullOrEmpty(scheme.DisplayName)
            select scheme
        ).ToArray();
    }

    public static async Task<bool> IsProviderSupported(this HttpContext context, string provider)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        return (
            from scheme in await context.GetExternalProviders()
            where string.Equals(scheme.Name, provider, StringComparison.OrdinalIgnoreCase)
            select scheme
        ).Any();
    }
}
