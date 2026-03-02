using ExchangeMonitor.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExchangeMonitor.Worker;

public class OutboxProcessorWorker : BackgroundService
{
    private readonly ILogger<OutboxProcessorWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private const int IntervalInSeconds = 60;

    public OutboxProcessorWorker(ILogger<OutboxProcessorWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Worker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var messages = await outboxRepository.GetUnprocessedMessagesAsync(batchSize: 10, stoppingToken);

                    foreach (var message in messages)
                    {
                        try
                        {
                            _logger.LogInformation("Processing Outbox Message {Id} of type {Type}", message.Id, message.Type);
                            await emailService.SendAlertEmailAsync("Exchange Rate Alert", message.Content, stoppingToken);
                            message.MarkAsProcessed();
                            await outboxRepository.UpdateAsync(message, stoppingToken);
                            await unitOfWork.CommitAsync(stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to process Outbox Message {Id}", message.Id);
                            message.MarkAsFailed(ex.Message);
                            await outboxRepository.UpdateAsync(message, stoppingToken);
                            await unitOfWork.CommitAsync(stoppingToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in Outbox Processor.");
            }

            await Task.Delay(TimeSpan.FromSeconds(IntervalInSeconds), stoppingToken);
        }
    }
}
