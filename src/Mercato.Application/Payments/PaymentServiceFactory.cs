using Mercato.Application.Common.Interfaces;
using Mercato.Domain.Enums;

namespace Mercato.Infrastructure.Services.Payments;

public class PaymentServiceFactory : IPaymentServiceFactory
{
    private readonly IEnumerable<IPaymentService> _paymentServices;

    public PaymentServiceFactory(IEnumerable<IPaymentService> paymentServices)
    {
        _paymentServices = paymentServices;
    }

    public IPaymentService GetService(PaymentProvider provider)
    {
        var service = _paymentServices.FirstOrDefault(x => x.Provider == provider);

        if (service is null)
        {
            throw new InvalidOperationException($"Payment provider '{provider}' is not registered.");
        }

        return service;
    }
}