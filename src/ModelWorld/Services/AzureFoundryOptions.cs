namespace ModelWorld.Services;

public sealed class AzureFoundryOptions
{
    public const string SectionName = "ModelWorld:Azure";
    public const string DefaultTokenScope = "https://ai.azure.com/.default";

    public string? Endpoint { get; init; }
    public int MaxOutputTokenCount { get; init; } = 300;
    public float Temperature { get; init; } = 0.2f;
    public int RequestTimeoutSeconds { get; init; } = 120;
    public Dictionary<string, string> DeploymentOverrides { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Uri GetNormalizedEndpoint()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            throw new InvalidOperationException("Live Azure mode requires ModelWorld:Azure:Endpoint. Set it with user secrets or an environment variable such as ModelWorld__Azure__Endpoint.");
        }

        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out var endpoint))
        {
            throw new InvalidOperationException("ModelWorld:Azure:Endpoint must be an absolute URI.");
        }

        if (endpoint.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("ModelWorld:Azure:Endpoint must use HTTPS.");
        }

        var endpointText = endpoint.ToString().TrimEnd('/');
        if (!endpointText.EndsWith("/openai/v1", StringComparison.OrdinalIgnoreCase))
        {
            endpointText += "/openai/v1";
        }

        return new Uri(endpointText + "/", UriKind.Absolute);
    }

    public TimeSpan GetRequestTimeout()
    {
        if (RequestTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("ModelWorld:Azure:RequestTimeoutSeconds must be greater than zero.");
        }

        return TimeSpan.FromSeconds(RequestTimeoutSeconds);
    }

    public int GetMaxOutputTokenCount()
    {
        if (MaxOutputTokenCount <= 0)
        {
            throw new InvalidOperationException("ModelWorld:Azure:MaxOutputTokenCount must be greater than zero.");
        }

        return MaxOutputTokenCount;
    }

    public float GetTemperature()
    {
        if (Temperature < 0 || Temperature > 2)
        {
            throw new InvalidOperationException("ModelWorld:Azure:Temperature must be between 0 and 2.");
        }

        return Temperature;
    }

    public string GetDeploymentName(ModelWorld.Models.ModelProfile model) =>
        DeploymentOverrides.TryGetValue(model.Id, out var overrideName) && !string.IsNullOrWhiteSpace(overrideName)
            ? overrideName
            : model.DeploymentName;
}
