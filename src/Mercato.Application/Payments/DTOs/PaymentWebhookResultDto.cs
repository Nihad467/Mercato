using Mercato.Domain.Enums;

namespace Mercato.Application.Payments.DTOs;

public class PaymentWebhookResultDto
{
    public bool IsSuccess { get; set; }
    public int OrderId { get; set; }
    public string ExternalPaymentId { get; set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; set; }
    public string? FailureReason { get; set; }
}