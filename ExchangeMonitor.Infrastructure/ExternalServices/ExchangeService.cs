using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using ExchangeMonitor.Domain.Entities;
using ExchangeMonitor.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace ExchangeMonitor.Infrastructure.ExternalServices;

public class ExchangeService : IExchangeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExchangeService> _logger;
    private readonly ResiliencePipeline _resiliencePipeline;

    public ExchangeService(HttpClient httpClient, ILogger<ExchangeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 5,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(5),
                OnRetry = args =>
                {
                    _logger.LogWarning("Retry attempt {Attempt}. Error: {Error}", args.AttemptNumber + 1, args.Outcome.Exception?.Message);
                    return default;
                }
            })
            .Build();
    }

    public async Task<ExchangeRate> GetLatestRateAsync(string baseCurrency, string targetCurrency, CancellationToken cancellationToken = default)
    {
        return await _resiliencePipeline.ExecuteAsync(async ct =>
        {
            _logger.LogInformation("Fetching rate from external API for {Base}/{Target}", baseCurrency, targetCurrency);

            var response = await _httpClient.GetAsync($"https://economia.awesomeapi.com.br/last/{baseCurrency}-{targetCurrency}", ct);
            
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("API Rate Limit hit (429). Polly will retry...");
                throw new HttpRequestException("Rate limit exceeded", null, System.Net.HttpStatusCode.TooManyRequests);
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<ExchangeApiResponse>(ct);
            
            var key = $"{baseCurrency}{targetCurrency}".ToUpper();

            if (content == null || !content.ContainsKey(key))
            {
                _logger.LogError("Key {Key} not found in API response. Content: {Content}", key, await response.Content.ReadAsStringAsync(ct));
                throw new HttpRequestException($"Invalid response from exchange API. Key {key} not found.");
            }

            var data = content[key];
            return new ExchangeRate(baseCurrency, targetCurrency, decimal.Parse(data.bid, System.Globalization.CultureInfo.InvariantCulture));
        }, cancellationToken);
    }

    private class ExchangeApiResponse : System.Collections.Generic.Dictionary<string, ExchangeData> { }
    private class ExchangeData { public string bid { get; set; } = string.Empty; }
}
