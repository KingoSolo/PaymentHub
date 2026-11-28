using PaymentHub.Shared.Data;
using PaymentHub.Shared.Entities;
using PaymentHub.Api.DTOs;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PaymentHub.Shared.Messages;

namespace PaymentHub.Api.Endpoints;
public static class BatchEndpoints
{
    public static void MapBatchEndpoints(this WebApplication app)
    {
        app.MapPost("/batches",async(
            CreatePaymentBatchRequest request,
            AppDbContext dbContext
        )=>
        {
          PaymentBatch paymentBatch = new PaymentBatch
          {
             TotalPayments = request.PaymentRequests.Count,
             Payments = request.PaymentRequests.Select(item => new Payment
             {
                 RecipientAccount = item.RecipientAccount,
                 Amount = item.Amount,
                 Currency = item.Currency,
                 IdempotencyKey = item.IdempotencyKey,
             }).ToList()
          };

          var outboxMessages = paymentBatch.Payments.Select(payment => new OutboxMessage
          {
              PaymentId = payment.Id,
              Type = "PaymentQueued",
              Payload = JsonSerializer.Serialize(
                new PaymentQueuedMessage(payment.Id, paymentBatch.Id))
          }).ToList();

          dbContext.PaymentBatches.Add(paymentBatch);
          dbContext.OutboxMessages.AddRange(outboxMessages);
          await dbContext.SaveChangesAsync();

          return Results.Created($"/batches/{paymentBatch.Id}", new
          {
              batchId = paymentBatch.Id,
              status = paymentBatch.Status.ToString(),
              totalpayments = paymentBatch.TotalPayments
          });
        });


        app.MapGet("/batches/{id:guid}",async(
            Guid id,
            AppDbContext dbContext
        ) =>
        {
            var batch = await dbContext.PaymentBatches
                .AsNoTracking()
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);
            
            if (batch is null)
            {
                return Results.NotFound(new { message = "Payment batch not found." });
            }

            var response = new PaymentBatchResponse
            {
                Id = batch.Id,
                Status = batch.Status.ToString(),
                TotalPayments = batch.TotalPayments,
                CreatedAt = batch.CreatedAt,
                Payments = batch.Payments.Select(p => new PaymentResponse
                {
                    Id = p.Id,
                    RecipientAccount = p.RecipientAccount,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status.ToString(),
                    ErrorMessage = p.ErrorMessage
                }).ToList()
            };
            return Results.Ok(response);
        });


    }
}