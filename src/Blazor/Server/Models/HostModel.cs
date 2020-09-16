using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Samples.Blazor.Server.Controllers;
using Samples.Blazor.Server.Services;

namespace Samples.Blazor.Server.Models
{
    public class HostModel : PageModel
    {
        public bool IsServerSideBlazor { get; set; }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            IsServerSideBlazor = BlazorModeController.IsServerSideBlazor(HttpContext);
            var clientIdProvider = HttpContext.RequestServices.GetRequiredService<IClientIdProvider>();
            await clientIdProvider.ReadOrCreateAsync(HttpContext);
            await base.OnPageHandlerExecutionAsync(context, next);
        }
    }
}
