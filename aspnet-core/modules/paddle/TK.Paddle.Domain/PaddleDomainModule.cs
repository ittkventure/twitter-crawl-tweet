using TK.Paddle.Client;
using TK.Paddle.Domain.Shared;
using Volo.Abp.Modularity;

namespace TK.Paddle.Domain
{
    [DependsOn(
        typeof(PaddleDomainSharedModule),
        typeof(PaddleClientModule)
     )]
    public class PaddleDomainModule : AbpModule
    {

    }
}