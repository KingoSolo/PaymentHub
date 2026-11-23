using Microsoft.EntityFrameworkCore;
using PaymentHub.Shared.Entities;

namespace PaymentHub.Shared.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    { 
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasOne(p => p.Batch)
                  .WithMany(b => b.Payments)
                  .HasForeignKey(p => p.BatchId);

            entity.HasIndex(p => p.IdempotencyKey)
                  .IsUnique();

            entity.ToTable(t => 
                {t.HasCheckConstraint(
                name:"CK_Payment_Amount_Positive",
                sql: "\"Amount\" > 0"
                );

                t.HasCheckConstraint(
                    name:"CK_Payments_RecipientAccount_Digits",
                    sql: "\"RecipientAccount\" ~ '^[0-9]{10}$'"
                );

                t.HasCheckConstraint(
                    name: "CK_Payments_Currency_Format",
                    sql: "\"Currency\" ~ '^[A-Z]{3}$'"
                );
            });

            entity.Property(p => p.Amount)
                  .HasPrecision(18,2);

            entity.Property(p => p.RecipientAccount)
                  .IsRequired()
                  .HasMaxLength(10);

            entity.Property(p=>p.IdempotencyKey)
                  .IsRequired() 
                  .HasMaxLength(64);
            
            entity.Property(p=>p.Currency)
                  .IsRequired()
                  .HasMaxLength(3);

            entity.Property(p=>p.ErrorMessage)
                  .IsRequired(false);
            
            entity.Property(p=>p.BankTransactionReference)
                  .IsRequired(false);
                  
            
        });

        modelBuilder.Entity<PaymentBatch>(entity =>
        {
            entity.ToTable(t=>
            t.HasCheckConstraint(
                name:"CK_PaymentBatch_TotalPayments_Count",
                sql:"\"TotalPayments\" > 0 "
            ));

        });
    } 

    public DbSet<Payment> Payments{get;set;}
    public DbSet<PaymentBatch> PaymentBatches{get;set;}

    
}