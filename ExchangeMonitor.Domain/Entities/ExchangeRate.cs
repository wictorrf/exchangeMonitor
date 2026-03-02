using System;

namespace ExchangeMonitor.Domain.Entities;

public class ExchangeRate
{
    public Guid Id { get; private set; }
    public string BaseCurrency { get; private set; }
    public string TargetCurrency { get; private set; }
    public decimal Rate { get; private set; }
    public DateTime CapturedAt { get; private set; }

    public ExchangeRate(string baseCurrency, string targetCurrency, decimal rate)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency)) throw new ArgumentException("Base currency is required.");
        if (string.IsNullOrWhiteSpace(targetCurrency)) throw new ArgumentException("Target currency is required.");
        if (rate <= 0) throw new ArgumentException("Rate must be greater than zero.");

        Id = Guid.NewGuid();
        BaseCurrency = baseCurrency.ToUpper();
        TargetCurrency = targetCurrency.ToUpper();
        Rate = rate;
        CapturedAt = DateTime.UtcNow;
    }

    public decimal CalculateVariation(decimal previousRate)
    {
        if (previousRate <= 0) return 0;
        return ((Rate - previousRate) / previousRate) * 100;
    }
}
