using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace TK.Paddle.Application.Contracts
{
    [DependsOn(typeof(AbpDddApplicationContractsModule))]
    public class PaddleApplicationContractsModule : AbpModule
    {

    }
}