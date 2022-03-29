using System;
using System.Threading.Tasks;
using AlibabaCloud.OpenApiClient.Models;
using AlibabaCloud.SDK.Ecs20140526;
using AlibabaCloud.SDK.Ecs20140526.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tea;
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
        if (acceptLanguage == null) throw new ArgumentNullException(nameof(acceptLanguage));

        var request = new DescribeRegionsRequest
        {
            AcceptLanguage = acceptLanguage
        };

        try
        {
            var response = await ApiClient.DescribeRegionsAsync(request);

            Logger.LogDebug("Retrieved {count} regions with {language}.", response.Body.Regions.Region.Count, acceptLanguage);
            return response.Body;
        }
        catch (TeaException ex)
        {
            Logger.LogWarning(
                "Got an error while retrieving regions with {language}.{newLine}{message}",
                acceptLanguage, Environment.NewLine, ex.Message);
            throw;
        }
    }

    public virtual async Task<DescribeInstanceTypesResponseBody> DescribeInstanceTypesAsync()
    {
        var request = new DescribeInstanceTypesRequest();

        try
        {
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
        catch (TeaException ex)
        {
            Logger.LogWarning(
                "Got an error while retrieving instance types.{newLine}{message}",
                Environment.NewLine, ex.Message);
            throw;
        }
    }

    public virtual async Task<DescribeSpotPriceHistoryResponseBody> DescribeSpotPriceHistoryAsync(
        string regionId, string networkType, string instanceType, DateTime startTime, DateTime endTime)
    {
        if (regionId.IsNullOrEmpty()) throw new ArgumentNullException(nameof(regionId));
        if (networkType.IsNullOrEmpty()) throw new ArgumentNullException(nameof(networkType));
        if (instanceType.IsNullOrEmpty()) throw new ArgumentNullException(nameof(instanceType));

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

        try
        {
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
        catch (TeaException ex)
        {
            Logger.LogWarning(
                "Got an error while retrieving spot price history of {instanceType} at {regionId} from {startTime} to {endTime}.{newLine}{message}",
                instanceType, regionId, isoStartTime, isoEndTime, Environment.NewLine, ex.Message);
            throw;
        }
    }
}
