using MediatR;

namespace Mercato.Application.Coupons.Commands.DeleteCoupon;

public class DeleteCouponCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public DeleteCouponCommand(Guid id)
    {
        Id = id;
    }
}