namespace PaymentHub.Shared.Messages;

public record PaymentQueuedMessage(
    Guid PaymentId,
    Guid BatchId
);
