using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stl.DependencyInjection;
using Stl.Fusion.Internal;

namespace Samples.Caching.Common
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class SettingsAttribute : ServiceAttributeBase
    {
        public override void Register(IServiceCollection services, Type implementationType)
        {
            if (!typeof(ISettings).IsAssignableFrom(implementationType))
                throw Errors.MustImplement(implementationType, typeof(ISettings));
            services.AddSingleton(implementationType, c => {
                var settings = (ISettings) c.Activate(implementationType);
                var cfg = c.GetRequiredService<IConfiguration>();
                cfg.GetSection(settings.SectionName)?.Bind(settings);
                return settings;
            });
        }
    }
}
