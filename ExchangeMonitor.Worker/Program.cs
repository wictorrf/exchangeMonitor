using ExchangeMonitor.Application.Handlers;
using ExchangeMonitor.Domain.Interfaces;
using ExchangeMonitor.Infrastructure.ExternalServices;
using ExchangeMonitor.Infrastructure.Persistence.DB;
using ExchangeMonitor.Infrastructure.Persistence.Repositories;
using ExchangeMonitor.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// 1. Banco de Dados
builder.Services.AddDbContext<ExchangeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Repositórios e Unit of Work
builder.Services.AddScoped<IExchangeRepository, ExchangeRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 3. Serviços Externos
builder.Services.AddHttpClient<IExchangeService, ExchangeService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// 4. MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CheckExchangeRateHandler).Assembly));

// 5. Os Workers (Background Services)
builder.Services.AddHostedService<ExchangeMonitorWorker>();
builder.Services.AddHostedService<OutboxProcessorWorker>();

var host = builder.Build();
host.Run();
