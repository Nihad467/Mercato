using Mercato.Domain.Enums;

namespace Mercato.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public PaymentProvider Provider { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; }

    public string? ExternalPaymentId { get; set; }

    public string? CheckoutUrl { get; set; }

    public string? ClientSecret { get; set; }

    public string? FailureReason { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}