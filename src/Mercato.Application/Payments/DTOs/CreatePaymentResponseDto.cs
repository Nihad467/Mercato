using Mercato.Domain.Enums;

namespace Mercato.Application.Payments.DTOs;

public class CreatePaymentResponseDto
{
    public Guid PaymentId { get; set; }
    public int OrderId { get; set; }
    public PaymentProvider Provider { get; set; }
    public PaymentStatus Status { get; set; }
    public string? ExternalPaymentId { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? ClientSecret { get; set; }
    public string? FailureReason { get; set; }
}