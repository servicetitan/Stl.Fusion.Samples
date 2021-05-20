using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Samples.HelloBlazorServer.Services;
using Stl.CommandR;
using Stl.Fusion;
using Stl.Fusion.Extensions;

namespace Samples.HelloBlazorServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Fusion services
            var fusion = services.AddFusion();
            fusion.AddFusionTime(); // IFusionTime is one of built-in compute services you can use
            fusion.AddComputeService<CounterService>();
            fusion.AddComputeService<ChatService>();
            fusion.AddComputeService<ChatBotService>();
            fusion.AddComputeService<WeatherForecastService>();

            // This is just to make sure ChatBotService.StartAsync is called on startup
            services.AddHostedService(c => c.GetRequiredService<ChatBotService>());

            // Default update delay is 0.5s
            services.AddTransient<IUpdateDelayer>(_ => new UpdateDelayer(0.5));

            // Web
            services.AddRazorPages();
            services.AddServerSideBlazor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
