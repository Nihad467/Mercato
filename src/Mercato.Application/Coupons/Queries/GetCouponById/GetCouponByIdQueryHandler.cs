using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Coupons.Queries.GetCouponById;

public class GetCouponByIdQueryHandler : IRequestHandler<GetCouponByIdQuery, GetCouponByIdResult?>
{
    private readonly IApplicationDbContext _context;

    public GetCouponByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetCouponByIdResult?> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _context.GetCouponByIdAsync(request.Id, cancellationToken);

        if (coupon is null)
            return null;

        return new GetCouponByIdResult
        {
            Id = coupon.Id,
            Code = coupon.Code,
            DiscountType = coupon.DiscountType,
            DiscountValue = coupon.DiscountValue,
            ExpireDate = coupon.ExpireDate,
            UsageLimit = coupon.UsageLimit,
            UsedCount = coupon.UsedCount,
            IsActive = coupon.IsActive,
            CreatedAt = coupon.CreatedAt
        };
    }
}