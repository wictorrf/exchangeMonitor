using System.Threading;
using System.Threading.Tasks;
using ExchangeMonitor.Domain.Entities;

namespace ExchangeMonitor.Domain.Interfaces;

public interface IExchangeRepository
{
    Task<ExchangeRate?> GetLastCapturedRateAsync(string baseCurrency, string targetCurrency, CancellationToken cancellationToken = default);
    Task SaveRateAsync(ExchangeRate rate, CancellationToken cancellationToken = default);
}
