using Volo.Abp.Modularity;

namespace Extra.SpotMonitor.Tests;

[DependsOn(
    typeof(SpotMonitorModule)
)]
public class SpotMonitorTestModule : AbpModule
{
}
