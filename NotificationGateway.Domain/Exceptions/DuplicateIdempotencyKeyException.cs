using System;

namespace NotificationGateway.Domain.Exceptions;

public sealed class DuplicateIdempotencyKeyException : Exception
{
    public string IdempotencyKey { get; }

    public DuplicateIdempotencyKeyException(string idempotencyKey)
        : base($"A notification with idempotency key '{idempotencyKey}' already exists.")
    {
        IdempotencyKey = idempotencyKey;
    }
}
