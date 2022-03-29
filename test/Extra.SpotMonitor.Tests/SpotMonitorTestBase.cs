using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Volo.Abp;
using Xunit.Abstractions;

namespace Extra.SpotMonitor.Tests;

public class SpotMonitorTestBase : AbpIntegratedTest<SpotMonitorTestModule>
{
    public SpotMonitorTestBase()
    {
    }

    public SpotMonitorTestBase(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

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

    protected override void ConfigureLogging(ILoggingBuilder builder)
    {
        if (TestOutputHelper == null) return;

        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
#else
            .MinimumLevel.Information()
            .MinimumLevel.Override("Volo", LogEventLevel.Warning)
#endif
            .Enrich.FromLogContext()
            .WriteTo.TestOutput(TestOutputHelper, outputTemplate: Program.LogTemplate)
            .CreateLogger();

        builder.AddSerilog();
    }
}
