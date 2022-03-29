using System;
using System.Threading.Tasks;
using Extra.SpotMonitor.AliCloud;
using Tea;
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
    [InlineData("cn-hangzhou", "vpc", "ecs.t6-c1m4.large", "-7.00:00:00", "00:00:00")]
    [InlineData("cn-shanghai", "vpc", "ecs.hfr6.large", "-90.00:00:00", "00:00:00")]
    [InlineData("cn-beijing", "vpc", "ecs.xn4.small", "-02:00:00", "00:00:00")]
    [InlineData("cn-hangzhou", "classic", "ecs.xn4.small", "-02:00:00", "00:00:00", true)]
    [InlineData("cn-hangzhou", "classic", "ecs.v5-c1m1.large", "-01:00:00", "00:00:00", true)]
    public async Task RetrieveSpotPriceHistoryAsyncTest(string regionId, string networkType, string instanceType, string startOffset, string endOffset, bool empty = false)
    {
        var now = DateTime.Now.AddMinutes(-15); // Local time must be within 15 minutes of actual time
        var startTime = now + TimeSpan.Parse(startOffset);
        var endTime = now + TimeSpan.Parse(endOffset);
        var history = await AliCloudSpotRetriever.DescribeSpotPriceHistoryAsync(regionId, networkType, instanceType, startTime, endTime);
        Assert.Null(history.NextOffset);
        if (empty)
        {
            Assert.Empty(history.SpotPrices.SpotPriceType);
            Assert.Empty(history.Currency);
        }
        else
        {
            Assert.NotEmpty(history.SpotPrices.SpotPriceType);
            Assert.NotEmpty(history.Currency);
        }
    }

    [Theory]
    [InlineData("cn-hangzhou", "vpc", "ecs.hfr6.large", "-90.00:00:00", "-80.00:00:00", "the endTime must be great than 30 days before")]
    [InlineData("cn-hangzhou", "vpc", "ecs.hfr6.large", "00:00:00", "00:00:00", "the startTime must be lower than endTime")]
    [InlineData("cn-hangzhou", "vpc", "ecs.hfr6.large", "00:00:00", "00:45:00", "the endTime must be less than now")]
    [InlineData("unrecognized", "vpc", "ecs.hfr6.large", "-01:00:00", "00:00:00", "The specified RegionId does not exist")]
    [InlineData("cn-hangzhou", "unrecognized", "ecs.hfr6.large", "-01:00:00", "00:00:00", "The specified parameter \"NetworkType\"  must be member of vpc")]
    [InlineData("cn-hangzhou", "classic", "unrecognized", "-01:00:00", "00:00:00", "instance type not valid")]
    public async Task RetrieveSpotPriceHistoryAsyncFailureTest(string regionId, string networkType, string instanceType, string startOffset, string endOffset,
        string expectedMessage)
    {
        var now = DateTime.Now.AddMinutes(-15); // Local time must be within 15 minutes of actual time
        var startTime = now + TimeSpan.Parse(startOffset);
        var endTime = now + TimeSpan.Parse(endOffset);
        var exception = await Assert.ThrowsAsync<TeaException>(async () =>
        {
            await AliCloudSpotRetriever.DescribeSpotPriceHistoryAsync(regionId, networkType, instanceType, startTime, endTime);
        });
        Assert.Contains(expectedMessage, exception.Message);
    }
}
