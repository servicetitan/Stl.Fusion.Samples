using Microsoft.Extensions.DependencyInjection;
using Stl.Extensibility;
using Stl.Fusion;

namespace Samples.HelloCart.V1
{
    public class InMemoryModule : ModuleBase
    {
        public InMemoryModule(IServiceCollection services) : base(services) { }

        public override void Use()
        {
            Services.AddFusion(fusion => {
                fusion.AddComputeService<IProductService, InMemoryProductService>();
                fusion.AddComputeService<ICartService, InMemoryCartService>();
            });
        }
    }
}
