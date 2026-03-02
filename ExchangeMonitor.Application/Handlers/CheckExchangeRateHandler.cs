using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ExchangeMonitor.Application.Commands;
using ExchangeMonitor.Domain.Entities;
using ExchangeMonitor.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ExchangeMonitor.Application.Handlers;

public class CheckExchangeRateHandler : IRequestHandler<CheckExchangeRateCommand, Unit>
{
    private readonly IExchangeService _exchangeService;
    private readonly IExchangeRepository _repository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CheckExchangeRateHandler> _logger;
    private const decimal AlertThresholdPercentage = 0.01m; 

    public CheckExchangeRateHandler(
        IExchangeService exchangeService,
        IExchangeRepository repository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork,
        ILogger<CheckExchangeRateHandler> logger)
    {
        _exchangeService = exchangeService;
        _repository = repository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(CheckExchangeRateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking exchange rate for {Base} to {Target}", request.BaseCurrency, request.TargetCurrency);

        var currentRate = await _exchangeService.GetLatestRateAsync(request.BaseCurrency, request.TargetCurrency, cancellationToken);
        var lastRate = await _repository.GetLastCapturedRateAsync(request.BaseCurrency, request.TargetCurrency, cancellationToken);
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            if (lastRate != null)
            {
                var variation = currentRate.CalculateVariation(lastRate.Rate);
                _logger.LogInformation("Variation detected: {Variation}%", variation);

                if (Math.Abs(variation) >= AlertThresholdPercentage)
                {
                    _logger.LogWarning("High variation detected! Creating outbox message...");
                    
                    var emailData = new
                    {
                        Subject = "Exchange Rate Alert",
                        Body = $"The rate for {request.BaseCurrency}/{request.TargetCurrency} changed by {variation:F2}%. Current: {currentRate.Rate}"
                    };

                    var outboxMessage = new OutboxMessage(
                        type: "ExchangeRateAlert",
                        content: JsonSerializer.Serialize(emailData)
                    );

                    await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
                }
            }

            await _repository.SaveRateAsync(currentRate, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing exchange rate check. Rolling back...");
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        return Unit.Value;
    }
}
