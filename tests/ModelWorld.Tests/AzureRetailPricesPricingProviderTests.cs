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
                  "unitPrice": 3.00,
                  "retailPrice": 3.00,
                  "armRegionName": "eastus",
                  "meterName": "gpt 5.4 Input Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "gpt 5.4 Global Input",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                },
                {
                  "currencyCode": "USD",
                  "unitPrice": 12.00,
                  "retailPrice": 12.00,
                  "armRegionName": "eastus",
                  "meterName": "gpt 5.4 Output Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "gpt 5.4 Global Output",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-15T00:00:00Z"
                }
              ],
              "NextPageLink": null
            }
            """);
        var provider = CreateProvider(handler);

        var pricing = await provider.GetPricingAsync([model], "eastus");

        var modelPricing = pricing[model.Id];
        Assert.True(modelPricing.IsAvailable);
        Assert.Equal(3.00m, modelPricing.InputCostPerMillionTokensUsd);
        Assert.Equal(12.00m, modelPricing.OutputCostPerMillionTokensUsd);
        Assert.Equal("eastus", modelPricing.Region);
        Assert.Equal(new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero), modelPricing.EffectiveStartDate);
        Assert.Contains("$filter=", handler.Requests.Single().Query, StringComparison.OrdinalIgnoreCase);
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
                  "unitPrice": 3.00,
                  "retailPrice": 3.00,
                  "armRegionName": "eastus",
                  "meterName": "gpt 5.4 Input Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "gpt 5.4 Global Input",
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
                  "unitPrice": 12.00,
                  "retailPrice": 12.00,
                  "armRegionName": "eastus",
                  "meterName": "gpt 5.4 Output Tokens",
                  "productName": "Azure OpenAI GPT5",
                  "skuName": "gpt 5.4 Global Output",
                  "priceType": "Consumption",
                  "unitOfMeasure": "1M Tokens",
                  "effectiveStartDate": "2026-06-01T00:00:00Z"
                }
              ],
              "NextPageLink": null
            }
            """);
        var provider = CreateProvider(handler);

        var pricing = await provider.GetPricingAsync([model], "eastus");

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
                  "armRegionName": "eastus",
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
                  "armRegionName": "eastus",
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

        var pricing = await provider.GetPricingAsync([model], "eastus");

        var modelPricing = pricing[model.Id];
        Assert.True(modelPricing.IsAvailable);
        Assert.Equal(1.1000m, modelPricing.InputCostPerMillionTokensUsd);
        Assert.Equal(4.4000m, modelPricing.OutputCostPerMillionTokensUsd);
    }

    [Fact]
    public async Task GetPricingAsync_ReturnsUnavailableWhenMetersDoNotMatch()
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
                  "armRegionName": "eastus",
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

        var pricing = await provider.GetPricingAsync([model], "eastus");

        Assert.False(pricing[model.Id].IsAvailable);
        Assert.Equal(0, pricing[model.Id].InputCostPerMillionTokensUsd);
        Assert.Contains("No confident", pricing[model.Id].Note, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPricingAsync_ThrowsWhenApiFails()
    {
        var model = ModelCatalog.GetById("gpt-54");
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError, "{});");
        var provider = CreateProvider(handler);

        await Assert.ThrowsAsync<HttpRequestException>(() => provider.GetPricingAsync([model], "eastus"));
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