using System.ComponentModel.DataAnnotations;

namespace PaymentHub.Api.DTOs;

public class CreatePaymentBatchRequest
{
    [Required]
    [MinLength(1,ErrorMessage ="A payment batch must have at least one payment")]
    public required List<CreatePaymentRequest> PaymentRequests {get;set;} 
}