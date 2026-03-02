using System;

namespace ExchangeMonitor.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }

    public OutboxMessage(string type, string content)
    {
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type is required.");
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content is required.");

        Id = Guid.NewGuid();
        Type = type;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
    }
}
