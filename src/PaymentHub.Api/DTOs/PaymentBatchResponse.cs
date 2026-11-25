namespace PaymentHub.Api.DTOs;

public class PaymentBatchResponse
{
    public Guid Id { get; set; }
    public required string Status { get; set; }
    public int TotalPayments { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public required List<PaymentResponse> Payments { get; set; }
}