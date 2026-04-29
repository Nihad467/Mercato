using MediatR;

namespace Mercato.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<CreateOrderResult>
{
    public string? CouponCode { get; set; }
}