using PaymentHub.Shared.Enums;

namespace PaymentHub.Shared.Entities;
public class Payment
{
    public Guid Id{get;set;} = Guid.NewGuid();
    public  Guid BatchId{get;set;}   
    public PaymentBatch Batch{get;set;} = null!;
    public PaymentStatus Status{get;set;} = PaymentStatus.Pending;
    public required string RecipientAccount{get;set;}
    public decimal Amount{get;set;}
    public string Currency{get;set;} = "NGN";
    public required string IdempotencyKey{get;set;} 
    public int RetryCount{get;set;}
    public DateTimeOffset CreatedAt{get;set;} = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt{get;set;} = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessingStartedAt { get; set; }
    public string? BankTransactionReference{get;set;}
    public string? ErrorMessage{get;set;}
}