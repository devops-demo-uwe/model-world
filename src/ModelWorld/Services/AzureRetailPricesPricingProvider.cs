using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ModelWorld.Models;

namespace ModelWorld.Services;

public sealed class AzureRetailPricesPricingProvider : IPricingProvider
{
    public const string SourceName = "Azure Retail Prices API";

    private readonly HttpClient httpClient;
    private readonly Uri pricingEndpoint;

    public AzureRetailPricesPricingProvider(HttpClient httpClient, Uri pricingEndpoint)
    {
        this.httpClient = httpClient;
        this.pricingEndpoint = pricingEndpoint;
    }

    public async Task<IReadOnlyDictionary<string, ModelPricing>> GetPricingAsync(
        IReadOnlyList<ModelProfile> models,
        string region,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(region);

        var normalizedRegion = region.Trim().ToLowerInvariant();
        Dictionary<string, ModelPricing> pricingByModelId = new(StringComparer.OrdinalIgnoreCase);

        foreach (var model in models)
        {
            var meters = await FetchMetersAsync(model, normalizedRegion, cancellationToken);
            pricingByModelId[model.Id] = ResolvePricing(model, meters, normalizedRegion);
        }

        return pricingByModelId;
    }

    private async Task<IReadOnlyList<RetailPriceMeter>> FetchMetersAsync(
        ModelProfile model,
        string region,
        CancellationToken cancellationToken)
    {
        List<RetailPriceMeter> meters = [];
        Uri? nextPage = CreateInitialQuery(model, region);

        while (nextPage is not null)
        {
            var response = await httpClient.GetFromJsonAsync<RetailPricesResponse>(nextPage, cancellationToken)
                ?? throw new InvalidOperationException("Azure Retail Prices API returned an empty response.");

            if (response.Items is not null)
            {
                meters.AddRange(response.Items);
            }

            nextPage = string.IsNullOrWhiteSpace(response.NextPageLink)
                ? null
                : new Uri(response.NextPageLink, UriKind.Absolute);
        }

        return meters;
    }

    private Uri CreateInitialQuery(ModelProfile model, string region)
    {
        var hints = model.PricingLookupHints.ProductNameContains
            .Concat(model.PricingLookupHints.SkuNameContains)
            .Where(hint => !string.IsNullOrWhiteSpace(hint))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(hint => $"contains(productName,'{EscapeODataString(hint)}') or contains(skuName,'{EscapeODataString(hint)}') or contains(meterName,'{EscapeODataString(hint)}')")
            .ToArray();

        var modelFilter = string.Join(" or ", hints);
        var filter = $"armRegionName eq '{EscapeODataString(region)}' and ({modelFilter})";

        return AppendQuery(pricingEndpoint, "$filter", filter);
    }

    private static ModelPricing ResolvePricing(ModelProfile model, IReadOnlyList<RetailPriceMeter> meters, string region)
    {
        var inputMeter = FindBestMeter(model, meters, model.PricingLookupHints.InputMeterNameContains);
        var outputMeter = FindBestMeter(model, meters, model.PricingLookupHints.OutputMeterNameContains);

        if (inputMeter is null || outputMeter is null)
        {
            return ModelPricing.Unavailable(
                model,
                SourceName,
                region,
                "No confident input/output token meter match was found.");
        }

        var effectiveStartDate = MaxDate(inputMeter.EffectiveStartDate, outputMeter.EffectiveStartDate);

        return ModelPricing.Available(
            model,
            GetPerMillionTokenPrice(inputMeter),
            GetPerMillionTokenPrice(outputMeter),
            SourceName,
            region,
            effectiveStartDate);
    }

    private static RetailPriceMeter? FindBestMeter(
        ModelProfile model,
        IReadOnlyList<RetailPriceMeter> meters,
        IReadOnlyList<string> directionHints) =>
        meters
            .Select(meter => new { Meter = meter, Score = ScoreMeter(model, meter, directionHints) })
            .Where(candidate => candidate.Score >= 5)
            .OrderByDescending(candidate => candidate.Score)
            .ThenByDescending(candidate => candidate.Meter.EffectiveStartDate)
            .FirstOrDefault()
            ?.Meter;

    private static int ScoreMeter(ModelProfile model, RetailPriceMeter meter, IReadOnlyList<string> directionHints)
    {
        if (!string.Equals(meter.CurrencyCode, "USD", StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(meter.PriceType) && !string.Equals(meter.PriceType, "Consumption", StringComparison.OrdinalIgnoreCase))
            || meter.UnitPrice <= 0)
        {
            return 0;
        }

        var productName = meter.ProductName ?? string.Empty;
        var skuName = meter.SkuName ?? string.Empty;
        var meterName = meter.MeterName ?? string.Empty;
        var unitOfMeasure = meter.UnitOfMeasure ?? string.Empty;
        var searchableText = string.Join(' ', productName, skuName, meterName);

        var score = 0;
        if (searchableText.Contains("batch", StringComparison.OrdinalIgnoreCase)
            || searchableText.Contains("cached", StringComparison.OrdinalIgnoreCase)
            || searchableText.Contains("hosting", StringComparison.OrdinalIgnoreCase)
            || searchableText.Contains("training", StringComparison.OrdinalIgnoreCase))
        {
            score -= 4;
        }

        if (ContainsAny(productName, model.PricingLookupHints.ProductNameContains))
        {
            score += 2;
        }

        if (ContainsAny(searchableText, model.PricingLookupHints.SkuNameContains))
        {
            score += 3;
        }

        if (ContainsAny(searchableText, directionHints))
        {
            score += 3;
        }

        if (ContainsAny(searchableText, model.PricingLookupHints.DeploymentTypeContains))
        {
            score += 1;
        }

        if (unitOfMeasure.Contains("1K", StringComparison.OrdinalIgnoreCase)
            || unitOfMeasure.Contains("1M", StringComparison.OrdinalIgnoreCase)
            || meterName.Contains("token", StringComparison.OrdinalIgnoreCase))
        {
            score += 1;
        }

        return score;
    }

    private static decimal GetPerMillionTokenPrice(RetailPriceMeter meter)
    {
        var unitOfMeasure = meter.UnitOfMeasure ?? string.Empty;
        if (unitOfMeasure.Contains("1K", StringComparison.OrdinalIgnoreCase))
        {
            return meter.UnitPrice * 1_000m;
        }

        return meter.UnitPrice;
    }

    private static bool ContainsAny(string text, IReadOnlyList<string> values) =>
        values.Any(value => text.Contains(value, StringComparison.OrdinalIgnoreCase));

    private static DateTimeOffset? MaxDate(DateTimeOffset? left, DateTimeOffset? right)
    {
        if (left is null)
        {
            return right;
        }

        if (right is null)
        {
            return left;
        }

        return left > right ? left : right;
    }

    private static Uri AppendQuery(Uri uri, string name, string value)
    {
        var separator = string.IsNullOrEmpty(uri.Query) ? "?" : "&";
        return new Uri(uri + separator + name + "=" + Uri.EscapeDataString(value), UriKind.Absolute);
    }

    private static string EscapeODataString(string value) =>
        value.Replace("'", "''", StringComparison.Ordinal);

    private sealed record RetailPricesResponse(
        [property: JsonPropertyName("Items")] IReadOnlyList<RetailPriceMeter>? Items,
        [property: JsonPropertyName("NextPageLink")] string? NextPageLink);

    private sealed record RetailPriceMeter(
        [property: JsonPropertyName("currencyCode")] string? CurrencyCode,
        [property: JsonPropertyName("unitPrice")] decimal UnitPrice,
        [property: JsonPropertyName("retailPrice")] decimal RetailPrice,
        [property: JsonPropertyName("armRegionName")] string? ArmRegionName,
        [property: JsonPropertyName("meterName")] string? MeterName,
        [property: JsonPropertyName("productName")] string? ProductName,
        [property: JsonPropertyName("skuName")] string? SkuName,
        [property: JsonPropertyName("priceType")] string? PriceType,
        [property: JsonPropertyName("unitOfMeasure")] string? UnitOfMeasure,
        [property: JsonPropertyName("effectiveStartDate")] DateTimeOffset? EffectiveStartDate);
}