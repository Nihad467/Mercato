using Mercato.Domain.Enums;

namespace Mercato.Application.Common.Interfaces;

public interface IPaymentServiceFactory
{
    IPaymentService GetService(PaymentProvider provider);
}