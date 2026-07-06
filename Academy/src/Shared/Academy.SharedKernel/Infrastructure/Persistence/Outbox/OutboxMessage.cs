namespace Academy.src.Shared.Academy.SharedKernel.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(
        Guid id,
        DateTimeOffset occurredAtUtc,
        string type,
        string content)
    {
        Id = id;
        OccurredAtUtc = occurredAtUtc;
        Type = type;
        Content = content;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset? ProcessedAtUtc { get; private set; }
    public string? Error { get; private set; }

    public static OutboxMessage Create(Guid id, DateTimeOffset occurredAtUtc, string type, string content)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Outbox message id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Outbox message type is required.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Outbox message content is required.", nameof(content));
        }

        return new OutboxMessage(id, occurredAtUtc, type, content);
    }

    public void MarkProcessed(DateTimeOffset processedAtUtc)
    {
        ProcessedAtUtc = processedAtUtc;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("Outbox error is required.", nameof(error));
        }

        Error = error;
    }
}
