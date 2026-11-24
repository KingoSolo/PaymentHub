using System.ComponentModel.DataAnnotations;

namespace PaymentHub.Api.DTOs;

public class CreatePaymentRequest
{
    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage ="Recipient Account must be exactly 10 ")]
    public required string RecipientAccount {get;set;}

    [Range(0.01,double.MaxValue,ErrorMessage ="Amount must be greater than 0 and not negative")]
    public decimal Amount{get;set;}

    [Required]
    [RegularExpression(@"^[A-Z]{3}$",ErrorMessage ="Currency must have at least three characters")]
    public string Currency{get;set;} = "NGN";

    [Required]
    public required string IdempotencyKey{get;set;}
}