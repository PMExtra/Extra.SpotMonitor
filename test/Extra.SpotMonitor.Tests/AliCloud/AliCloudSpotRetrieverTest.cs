using System;
using System.Threading.Tasks;
using Extra.SpotMonitor.AliCloud;
using Xunit;
using Xunit.Abstractions;

namespace Extra.SpotMonitor.Tests.AliCloud;

public sealed class AliCloudSpotRetrieverTest : SpotMonitorTestBase
{
    public AliCloudSpotRetrieverTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        AliCloudSpotRetriever = GetRequiredService<AliCloudSpotRetriever>();
    }

    private AliCloudSpotRetriever AliCloudSpotRetriever { get; }

    [Fact]
    public async Task RetrieveRegionsAsyncTest()
    {
        foreach (var language in AliCloudLanguage.GetAllLanguages())
        {
            var regions = await AliCloudSpotRetriever.DescribeRegionsAsync(language);
            Assert.NotEmpty(regions.Regions.Region);
        }
    }

    [Fact]
    public async Task RetrieveInstanceTypesAsyncTest()
    {
        var types = await AliCloudSpotRetriever.DescribeInstanceTypesAsync();
        Assert.True(types.NextToken.IsNullOrEmpty());
        Assert.NotEmpty(types.InstanceTypes.InstanceType);
    }

    [Theory]
    [InlineData("cn-hangzhou", "vpc", "ecs.hfr6.large")]
    public async Task RetrieveSpotPriceHistoryAsyncTest(string regionId, string networkType, string instanceType)
    {
        var now = DateTime.Now;
        var endTime = now.Date.AddHours(now.Hour - 1);
        var startTime = endTime.AddDays(-7);
        var history = await AliCloudSpotRetriever.DescribeSpotPriceHistoryAsync(regionId, networkType, instanceType, startTime, endTime);
        Assert.Null(history.NextOffset);
        Assert.NotEmpty(history.SpotPrices.SpotPriceType);
        Assert.NotEmpty(history.Currency);
    }
}
