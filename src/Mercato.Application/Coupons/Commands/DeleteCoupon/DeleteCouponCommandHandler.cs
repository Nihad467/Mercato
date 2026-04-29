using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Coupons.Commands.DeleteCoupon;

public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _context.GetCouponByIdAsync(request.Id, cancellationToken);

        if (coupon is null)
            return false;

        _context.RemoveCoupon(coupon);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}