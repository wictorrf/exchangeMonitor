using System.Threading;
using System.Threading.Tasks;

namespace ExchangeMonitor.Domain.Interfaces;

public interface IEmailService
{
    Task SendAlertEmailAsync(string subject, string body, CancellationToken cancellationToken = default);
}
