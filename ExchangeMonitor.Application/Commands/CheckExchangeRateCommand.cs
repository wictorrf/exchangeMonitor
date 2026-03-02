using MediatR;

namespace ExchangeMonitor.Application.Commands;

public record CheckExchangeRateCommand(string BaseCurrency, string TargetCurrency) : IRequest<Unit>;
