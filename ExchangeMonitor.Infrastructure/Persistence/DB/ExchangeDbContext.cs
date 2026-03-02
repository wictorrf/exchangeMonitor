using ExchangeMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeMonitor.Infrastructure.Persistence.DB;

public class ExchangeDbContext : DbContext
{
    public ExchangeDbContext(DbContextOptions<ExchangeDbContext> options) : base(options) { }

    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExchangeRate>(b =>
        {
            b.ToTable("ExchangeRates");
            b.HasKey(e => e.Id);
            b.Property(e => e.BaseCurrency).HasMaxLength(3).IsRequired();
            b.Property(e => e.TargetCurrency).HasMaxLength(3).IsRequired();
            b.Property(e => e.Rate).HasPrecision(18, 4).IsRequired();
            b.Property(e => e.CapturedAt).IsRequired();
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");
            b.HasKey(e => e.Id);
            b.Property(e => e.Type).HasMaxLength(100).IsRequired();
            b.Property(e => e.Content).IsRequired();
            b.Property(e => e.CreatedAt).IsRequired();
            b.Property(e => e.ProcessedAt);
            b.Property(e => e.Error);
        });
    }
}
