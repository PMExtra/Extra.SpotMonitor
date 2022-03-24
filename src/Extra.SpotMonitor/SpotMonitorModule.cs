using System.Threading.Tasks;
using AlibabaCloud.OpenApiClient.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Extra.SpotMonitor;

[DependsOn(
    typeof(AbpAutofacModule)
)]
public class SpotMonitorModule : AbpModule
{
    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<Config>(configuration.GetSection("AliCloud"));
    }

    public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<SpotMonitorModule>>();

        var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();
        logger.LogInformation("EnvironmentName => {environmentName}", hostEnvironment.EnvironmentName);

        return Task.CompletedTask;
    }
}
