using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExchangeMonitor.Domain.Entities;
using ExchangeMonitor.Domain.Interfaces;
using ExchangeMonitor.Infrastructure.Persistence.DB;
using Microsoft.EntityFrameworkCore;

namespace ExchangeMonitor.Infrastructure.Persistence.Repositories;

public class ExchangeRepository : IExchangeRepository
{
    private readonly ExchangeDbContext _context;

    public ExchangeRepository(ExchangeDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRate?> GetLastCapturedRateAsync(string baseCurrency, string targetCurrency, CancellationToken cancellationToken = default)
    {
        return await _context.ExchangeRates
            .Where(r => r.BaseCurrency == baseCurrency.ToUpper() && r.TargetCurrency == targetCurrency.ToUpper())
            .OrderByDescending(r => r.CapturedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveRateAsync(ExchangeRate rate, CancellationToken cancellationToken = default)
    {
        await _context.ExchangeRates.AddAsync(rate, cancellationToken);
    }
}
