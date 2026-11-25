public class PaymentResponse
{
    public Guid Id { get; set; }
    public required string RecipientAccount { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required string Status { get; set; }
    public string? ErrorMessage { get; set; }
}
