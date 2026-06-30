using System.Net;
using ModelWorld.Catalogs;
using ModelWorld.Services;

namespace ModelWorld.Tests;

public sealed class AzureRetailPricesPricingProviderTests
{
    [Fact]
    public async Task GetPricingAsync_ResolvesInputAndOutputMeters()
    {
        var model = ModelCatalog.GetById("gpt-54");
        var handler = new FakeHttpMessageHandler(
            """
            {
              "Items": [
                {
                  "currencyCode": "USD",
                  "unitPrice": 2.50,
                  "retailPrice": 2.50,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 inp Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 inp Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 15.00,
                  "retailPrice": 15.00,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 opt Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 opt Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-15T00:00:00Z"
                }
              ],
              "NextPageLink": null
            }
            """);
        var provider = CreateProvider(handler);

        var pricing = await provider.GetPricingAsync([model]);

        var modelPricing = pricing[model.Id];
        Assert.True(modelPricing.IsAvailable);
        Assert.Equal(2.50m, modelPricing.InputCostPerMillionTokensUsd);
        Assert.Equal(15.00m, modelPricing.OutputCostPerMillionTokensUsd);
        Assert.Equal("swedencentral", modelPricing.Region);
        Assert.Equal(new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero), modelPricing.EffectiveStartDate);
        Assert.Contains("$filter=", handler.Requests.Single().Query, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPricingAsync_UsesCatalogRegionAndRejectsNonGlobalStandardMeters()
    {
        var model = ModelCatalog.GetById("gpt-54");
        var handler = new FakeHttpMessageHandler(
            """
            {
              "Items": [
                {
                  "currencyCode": "USD",
                  "unitPrice": 1.25,
                  "retailPrice": 1.25,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 Batch inp Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 Batch inp Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 0.25,
                  "retailPrice": 0.25,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 cd inp Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 cd inp Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 2.75,
                  "retailPrice": 2.75,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 inp Dz 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 inp Dz",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 3.10,
                  "retailPrice": 3.10,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 inp regnl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 inp regnl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 5.00,
                  "retailPrice": 5.00,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 longco inp Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 longco inp Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 2.50,
                  "retailPrice": 2.50,
                  "armRegionName": "eastus",
                  "meterName": "5.4 inp Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 inp Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 2.50,
                  "retailPrice": 2.50,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 inp Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 inp Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 7.50,
                  "retailPrice": 7.50,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 Batch opt Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 Batch opt Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 16.50,
                  "retailPrice": 16.50,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 opt Dz 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 opt Dz",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 30.00,
                  "retailPrice": 30.00,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 pp opt Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 pp opt Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 22.50,
                  "retailPrice": 22.50,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 longco opt Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 longco opt Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 15.00,
                  "retailPrice": 15.00,
                  "armRegionName": "eastus",
                  "meterName": "5.4 opt Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 opt Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 15.00,
                  "retailPrice": 15.00,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 opt Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 opt Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                }
              ],
              "NextPageLink": null
            }
            """);
        var provider = CreateProvider(handler);

        var pricing = await provider.GetPricingAsync([model]);

        var modelPricing = pricing[model.Id];
        Assert.True(modelPricing.IsAvailable);
        Assert.Equal(2.50m, modelPricing.InputCostPerMillionTokensUsd);
        Assert.Equal(15.00m, modelPricing.OutputCostPerMillionTokensUsd);
        Assert.Equal("swedencentral", modelPricing.Region);
        Assert.Contains("swedencentral", Uri.UnescapeDataString(handler.Requests.Single().Query), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPricingAsync_RequiresSpecificModelTextBeforeMatching()
    {
        var model = ModelCatalog.GetById("gpt-54-mini");
        var handler = new FakeHttpMessageHandler(
            """
            {
              "Items": [
                {
                  "currencyCode": "USD",
                  "unitPrice": 2.50,
                  "retailPrice": 2.50,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 inp Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 inp Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 15.00,
                  "retailPrice": 15.00,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 opt Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 opt Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-07-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 0.75,
                  "retailPrice": 0.75,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 mini Inp Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 mini Inp Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 4.50,
                  "retailPrice": 4.50,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 mini Opt Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 mini Opt Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                }
              ],
              "NextPageLink": null
            }
            """);
        var provider = CreateProvider(handler);

        var pricing = await provider.GetPricingAsync([model]);

        var modelPricing = pricing[model.Id];
        Assert.True(modelPricing.IsAvailable);
        Assert.Equal(0.75m, modelPricing.InputCostPerMillionTokensUsd);
        Assert.Equal(4.50m, modelPricing.OutputCostPerMillionTokensUsd);
    }

    [Fact]
    public async Task GetPricingAsync_FlagsWhenApiPriceDiffersFromCatalogFallback()
    {
        var model = ModelCatalog.GetById("gpt-54-mini");
        var handler = new FakeHttpMessageHandler(
            """
            {
              "Items": [
                {
                  "currencyCode": "USD",
                  "unitPrice": 0.80,
                  "retailPrice": 0.80,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 mini Inp Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 mini Inp Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 4.50,
                  "retailPrice": 4.50,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 mini Opt Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 mini Opt Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                }
              ],
              "NextPageLink": null
            }
            """);
        var provider = CreateProvider(handler);

        var pricing = await provider.GetPricingAsync([model]);

        var modelPricing = pricing[model.Id];
        Assert.True(modelPricing.IsAvailable);
        Assert.Equal(AzureRetailPricesPricingProvider.SourceName, modelPricing.Source);
        Assert.Equal(0.80m, modelPricing.InputCostPerMillionTokensUsd);
        Assert.Equal(4.50m, modelPricing.OutputCostPerMillionTokensUsd);
        Assert.Contains("mismatch", modelPricing.Note, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPricingAsync_FollowsNextPageLink()
    {
        var model = ModelCatalog.GetById("gpt-54");
        var handler = new FakeHttpMessageHandler(
            """
            {
              "Items": [
                {
                  "currencyCode": "USD",
                  "unitPrice": 2.50,
                  "retailPrice": 2.50,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 inp Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 inp Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                }
              ],
              "NextPageLink": "https://prices.azure.com/api/retail/prices?page=2"
            }
            """,
            """
            {
              "Items": [
                {
                  "currencyCode": "USD",
                  "unitPrice": 15.00,
                  "retailPrice": 15.00,
                  "armRegionName": "swedencentral",
                  "meterName": "5.4 opt Gl 1M Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "5.4 opt Gl",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                }
              ],
              "NextPageLink": null
            }
            """);
        var provider = CreateProvider(handler);

        var pricing = await provider.GetPricingAsync([model]);

        Assert.True(pricing[model.Id].IsAvailable);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task GetPricingAsync_ConvertsOneThousandTokenMetersToPerMillionRates()
    {
        var model = ModelCatalog.GetById("o4-mini");
        var handler = new FakeHttpMessageHandler(
            """
            {
              "Items": [
                {
                  "currencyCode": "USD",
                  "unitPrice": 0.0011,
                  "retailPrice": 0.0011,
                  "armRegionName": "swedencentral",
                  "meterName": "o4-mini 0416 Inp glbl Tokens",
                  "productName": "Azure OpenAI Reasoning",
                  "skuName": "o4-mini 0416 Inp glbl",
                  "priceType": null,
                  "unitOfMeasure": "1K",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 0.0044,
                  "retailPrice": 0.0044,
                  "armRegionName": "swedencentral",
                  "meterName": "o4-mini 0416 Outp glbl Tokens",
                  "productName": "Azure OpenAI Reasoning",
                  "skuName": "o4-mini 0416 Outp glbl",
                  "priceType": null,
                  "unitOfMeasure": "1K",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                }
              ],
              "NextPageLink": null
            }
            """);
        var provider = CreateProvider(handler);

        var pricing = await provider.GetPricingAsync([model]);

        var modelPricing = pricing[model.Id];
        Assert.True(modelPricing.IsAvailable);
        Assert.Equal(1.1000m, modelPricing.InputCostPerMillionTokensUsd);
        Assert.Equal(4.4000m, modelPricing.OutputCostPerMillionTokensUsd);
    }

    [Fact]
    public async Task GetPricingAsync_UsesCatalogFallbackWhenMetersDoNotMatch()
    {
        var model = ModelCatalog.GetById("llama-33-70b-instruct");
        var handler = new FakeHttpMessageHandler(
            """
            {
              "Items": [
                {
                  "currencyCode": "USD",
                  "unitPrice": 1.00,
                  "retailPrice": 1.00,
                  "armRegionName": "swedencentral",
                  "meterName": "Unrelated Input Tokens",
                  "productName": "Unrelated Product",
                  "skuName": "Unrelated Global Input",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                }
              ],
              "NextPageLink": null
            }
            """);
        var provider = CreateProvider(handler);

        var pricing = await provider.GetPricingAsync([model]);

        var modelPricing = pricing[model.Id];
        Assert.True(modelPricing.IsAvailable);
        Assert.Equal(AzureRetailPricesPricingProvider.CatalogFallbackSourceName, modelPricing.Source);
        Assert.Equal(model.InputCostPerMillionTokensUsd, modelPricing.InputCostPerMillionTokensUsd);
        Assert.Equal(model.OutputCostPerMillionTokensUsd, modelPricing.OutputCostPerMillionTokensUsd);
        Assert.Contains("catalog fallback", modelPricing.Note, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
      public async Task GetPricingAsync_UsesCatalogFallbackForDeepSeekWhenOnlyDataZoneMetersExist()
    {
        var model = ModelCatalog.GetById("deepseek-v4-pro");
        var handler = new FakeHttpMessageHandler(
            """
            {
              "Items": [
                {
                  "currencyCode": "USD",
                  "unitPrice": 0.001925,
                  "retailPrice": 0.001925,
                  "armRegionName": "swedencentral",
                  "meterName": "FW DeepSeek-V4-Pro Inp DZ Tokens",
                  "productName": "Azure Fireworks Models",
                  "skuName": "FW DeepSeek-V4-Pro Inp DZ",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1K",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 0.003828,
                  "retailPrice": 0.003828,
                  "armRegionName": "swedencentral",
                  "meterName": "FW DeepSeek-V4-Pro Outp DZ Tokens",
                  "productName": "Azure Fireworks Models",
                  "skuName": "FW DeepSeek-V4-Pro Outp DZ",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1K",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                }
              ],
              "NextPageLink": null
            }
            """);
        var provider = CreateProvider(handler);

        var pricing = await provider.GetPricingAsync([model]);

        var modelPricing = pricing[model.Id];
        Assert.True(modelPricing.IsAvailable);
        Assert.Equal(AzureRetailPricesPricingProvider.CatalogFallbackSourceName, modelPricing.Source);
        Assert.Equal("swedencentral", modelPricing.Region);
        Assert.Equal(1.74m, modelPricing.InputCostPerMillionTokensUsd);
        Assert.Equal(3.48m, modelPricing.OutputCostPerMillionTokensUsd);
        Assert.Contains("catalog fallback", modelPricing.Note, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPricingAsync_ThrowsWhenApiFails()
    {
        var model = ModelCatalog.GetById("gpt-54");
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError, "{});");
        var provider = CreateProvider(handler);

        await Assert.ThrowsAsync<HttpRequestException>(() => provider.GetPricingAsync([model]));
    }

    private static AzureRetailPricesPricingProvider CreateProvider(FakeHttpMessageHandler handler) =>
        new(
            new HttpClient(handler),
            new Uri("https://prices.azure.com/api/retail/prices?api-version=2023-01-01-preview"));

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> responses;

        public FakeHttpMessageHandler(params string[] jsonResponses)
        {
            responses = new Queue<HttpResponseMessage>(jsonResponses.Select(CreateJsonResponse));
        }

        public FakeHttpMessageHandler(HttpStatusCode statusCode, string responseText)
        {
            responses = new Queue<HttpResponseMessage>(
            [
                new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(responseText)
                }
            ]);
        }

        public List<Uri> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!);
            return Task.FromResult(responses.Dequeue());
        }

        private static HttpResponseMessage CreateJsonResponse(string json) =>
            new(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
    }
}