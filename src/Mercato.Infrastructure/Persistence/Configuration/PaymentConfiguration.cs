using Mercato.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercato.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.ExternalPaymentId)
            .HasMaxLength(200);

        builder.Property(x => x.CheckoutUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.ClientSecret)
            .HasMaxLength(500);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(500);

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => x.ExternalPaymentId)
            .IsUnique()
            .HasFilter("[ExternalPaymentId] IS NOT NULL");

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}