using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Testing;

namespace Extra.SpotMonitor.Tests;

public class SpotMonitorTestBase : AbpIntegratedTest<SpotMonitorTestModule>
{
    protected override void BeforeAddApplication(IServiceCollection services)
    {
        services.ReplaceConfiguration(InitConfiguration());
    }

    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }

    protected virtual IConfiguration InitConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<SpotMonitorTestModule>()
            .AddEnvironmentVariables()
            .Build();
        return configuration;
    }
}
