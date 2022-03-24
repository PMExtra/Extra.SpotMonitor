using System;
using System.Threading.Tasks;
using AlibabaCloud.OpenApiClient.Models;
using AlibabaCloud.SDK.Ecs20140526;
using AlibabaCloud.SDK.Ecs20140526.Models;
using Extra.SpotMonitor.AliCloud.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace Extra.SpotMonitor.AliCloud;

public class AliCloudSpotRetriever : ITransientDependency
{
    public AliCloudSpotRetriever(IOptions<Config> config)
    {
        Logger = NullLogger<AliCloudSpotRetriever>.Instance;
        ApiClient = new Client(config.Value);
    }

    protected Client ApiClient { get; }

    public ILogger<AliCloudSpotRetriever> Logger { get; set; }

    public virtual async Task<DescribeRegionsResponseBody> DescribeRegionsAsync(AliCloudLanguage acceptLanguage)
    {
        var request = new DescribeRegionsRequest
        {
            AcceptLanguage = acceptLanguage
        };
        var response = await ApiClient.DescribeRegionsAsync(request);

        Logger.LogDebug("Retrieved {count} regions with {language}.", response.Body.Regions.Region.Count, acceptLanguage);
        return response.Body;
    }

    public virtual async Task<DescribeInstanceTypesResponseBody> DescribeInstanceTypesAsync()
    {
        var request = new DescribeInstanceTypesRequest();
        var response = await ApiClient.DescribeInstanceTypesAsync(request);
        request.NextToken = response.Body.NextToken;
        response.Body.NextToken = string.Empty;
        while (request.NextToken is { Length: > 0 })
        {
            var next = await ApiClient.DescribeInstanceTypesAsync(request);
            response.Body.InstanceTypes.InstanceType.AddRange(next.Body.InstanceTypes.InstanceType);
            request.NextToken = next.Body.NextToken;
        }

        Logger.LogDebug("Retrieved {count} instance types.", response.Body.InstanceTypes.InstanceType.Count);
        return response.Body;
    }

    public virtual async Task<DescribeSpotPriceHistoryResponseBody> DescribeSpotPriceHistoryAsync(
        string regionId, string networkType, string instanceType, DateTime startTime, DateTime endTime)
    {
        var isoStartTime = startTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        var isoEndTime = endTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

        var request = new DescribeSpotPriceHistoryRequest
        {
            RegionId = regionId,
            NetworkType = networkType,
            InstanceType = instanceType,
            StartTime = isoStartTime,
            EndTime = isoEndTime
        };
        var response = await ApiClient.DescribeSpotPriceHistoryAsync(request);
        request.Offset = response.Body.NextOffset;
        response.Body.NextOffset = null;
        while (request.Offset.HasValue)
        {
            var next = await ApiClient.DescribeSpotPriceHistoryAsync(request);
            response.Body.SpotPrices.SpotPriceType.AddRange(next.Body.SpotPrices.SpotPriceType);
            request.Offset = next.Body.NextOffset;
        }

        Logger.LogDebug(
            "Retrieved {count} spot price records of {instanceType} at {regionId} from {startTime} to {endTime}.",
            response.Body.SpotPrices.SpotPriceType.Count, instanceType, regionId, isoStartTime, isoEndTime);
        return response.Body;
    }
}
