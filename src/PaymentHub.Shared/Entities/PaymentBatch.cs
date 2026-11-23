using PaymentHub.Shared.Enums;
namespace PaymentHub.Shared.Entities;

public class PaymentBatch
{
    public Guid Id {get;set;} =  Guid.NewGuid(); 
    public BatchStatus Status{get;set;} = BatchStatus.Pending;
    public DateTimeOffset CreatedAt{get;set;} = DateTimeOffset.UtcNow;
    public int TotalPayments {get;set;}
    public ICollection<Payment> Payments{get;set;} = [];
}