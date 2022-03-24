using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Extra.SpotMonitor.Tests;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(SpotMonitorModule)
)]
public class SpotMonitorTestModule : AbpModule
{
}
