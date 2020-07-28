using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Samples.Blazor.Server.Services
{
    public interface IBlazorModeSwitcher
    {
        public bool IsServerSideBlazor { get; set; }
        public bool TrySwitchMode();
    }

    public class BlazorModeSwitcher : IBlazorModeSwitcher
    {
        public class Options
        {
            public string CookieName { get; set; } = "_ssb_";
            public string ParameterName { get; set; } = "serverSideBlazor";
        }

        private IHttpContextAccessor HttpContextAccessor { get; }
        public string CookieName { get; }
        public string ParameterName { get; }

        public bool IsServerSideBlazor {
            get {
                var cookies = HttpContextAccessor.HttpContext.Request.Cookies;
                var ssb = cookies.TryGetValue(CookieName, out var v1) ? v1 : "";
                return int.TryParse(ssb, out var v2) && v2 != 0;
            }
            set {
                var response = HttpContextAccessor.HttpContext.Response;
                response.Cookies.Append(CookieName, Convert.ToInt32(value).ToString());
            }
        }

        public BlazorModeSwitcher(IHttpContextAccessor httpContextAccessor)
            : this(null, httpContextAccessor) { }

        public BlazorModeSwitcher(
            Options? options,
            IHttpContextAccessor httpContextAccessor)
        {
            options ??= new Options();
            CookieName = options.CookieName;
            ParameterName = options.ParameterName;
            HttpContextAccessor = httpContextAccessor;
        }

        public bool TrySwitchMode()
        {
            var query = HttpContextAccessor.HttpContext.Request.Query;
            if (!query.TryGetValue(ParameterName, out var ssb))
                return false;
            var isSsb = int.TryParse(ssb.SingleOrDefault() ?? "", out var v2) && v2 != 0;
            if (IsServerSideBlazor == isSsb)
                return false;
            IsServerSideBlazor = isSsb;
            return true;
        }
    }
}
