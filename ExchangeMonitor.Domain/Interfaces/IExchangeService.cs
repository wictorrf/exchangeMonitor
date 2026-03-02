using System.Threading;
using System.Threading.Tasks;
using ExchangeMonitor.Domain.Entities;

namespace ExchangeMonitor.Domain.Interfaces;

public interface IExchangeService
{
    Task<ExchangeRate> GetLatestRateAsync(string baseCurrency, string targetCurrency, CancellationToken cancellationToken = default);
}
