using ExchangeMonitor.Application.Commands;
using MediatR;

namespace ExchangeMonitor.Worker;

public class ExchangeMonitorWorker : BackgroundService
{
    private readonly ILogger<ExchangeMonitorWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private const int IntervalInMinutes = 1;

    public ExchangeMonitorWorker(ILogger<ExchangeMonitorWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Exchange Monitor Worker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Running exchange rate check...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                
                    await mediator.Send(new CheckExchangeRateCommand("BRL", "ARS"), stoppingToken);
                }

                _logger.LogInformation("Exchange rate check completed. Waiting {minutes} minutes...", IntervalInMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking exchange rates.");
            }

            await Task.Delay(TimeSpan.FromMinutes(IntervalInMinutes), stoppingToken);
        }
    }
}
