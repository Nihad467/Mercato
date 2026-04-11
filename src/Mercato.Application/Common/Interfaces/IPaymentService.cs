using Mercato.Application.Payments.DTOs;
using Mercato.Domain.Entities;
using Mercato.Domain.Enums;

namespace Mercato.Application.Common.Interfaces;

public interface IPaymentService
{
    PaymentProvider Provider { get; }

    Task<CreatePaymentResponseDto> CreatePaymentAsync(Order order, CancellationToken cancellationToken);
}