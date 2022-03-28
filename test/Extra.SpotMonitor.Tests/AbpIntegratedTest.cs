using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit.Abstractions;

namespace Extra.SpotMonitor.Tests;

// TODO: Replace this temporary solution, blocked by https://github.com/abpframework/abp/issues/12092
[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
public abstract class AbpIntegratedTest<TStartupModule> : AbpTestBaseWithServiceProvider, IDisposable
    where TStartupModule : IAbpModule
{
    protected AbpIntegratedTest() : this(null)
    {
    }

    protected AbpIntegratedTest(ITestOutputHelper? testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;

        var services = CreateServiceCollection();

        BeforeAddApplication(services);

        var application = services.AddApplication<TStartupModule>(SetAbpApplicationCreationOptions);
        Application = application;

        AfterAddApplication(services);

        services.AddLogging(ConfigureLogging);

        RootServiceProvider = CreateServiceProvider(services);
        TestServiceScope = RootServiceProvider.CreateScope();

        application.Initialize(TestServiceScope.ServiceProvider);
        ServiceProvider = Application.ServiceProvider;
    }

    protected ITestOutputHelper? TestOutputHelper { get; }

    protected IAbpApplication Application { get; }

    protected IServiceProvider RootServiceProvider { get; }

    protected IServiceScope TestServiceScope { get; }

    public virtual void Dispose()
    {
        Application.Shutdown();
        TestServiceScope.Dispose();
        Application.Dispose();
    }

    protected virtual IServiceCollection CreateServiceCollection() => new ServiceCollection();

    protected virtual void BeforeAddApplication(IServiceCollection services)
    {
    }

    protected virtual void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
    }

    protected virtual void AfterAddApplication(IServiceCollection services)
    {
    }

    protected virtual void ConfigureLogging(ILoggingBuilder builder)
    {
    }

    protected virtual IServiceProvider CreateServiceProvider(IServiceCollection services) => services.BuildServiceProviderFromFactory();
}
