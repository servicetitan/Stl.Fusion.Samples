using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;

namespace Samples.HelloCart.V1
{
    public class InMemoryApp : AppBase
    {
        public InMemoryApp()
        {
            var services = new ServiceCollection();
            services.AddFusion(fusion => {
                fusion.AddComputeService<IProductService, InMemoryProductService>();
                fusion.AddComputeService<ICartService, InMemoryCartService>();
            });
            ClientServices = Services = services.BuildServiceProvider();
        }
    }
}
