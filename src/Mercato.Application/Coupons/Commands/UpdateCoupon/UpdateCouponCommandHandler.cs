using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Domain.Enums;

namespace Mercato.Application.Coupons.Commands.UpdateCoupon;

public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _context.GetCouponByIdAsync(request.Id, cancellationToken);

        if (coupon is null)
            return false;

        var code = request.Code.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(code))
            throw new Exception("Coupon code is required.");

        if (request.DiscountValue <= 0)
            throw new Exception("Discount value must be greater than zero.");

        if (request.DiscountType == DiscountType.Percentage && request.DiscountValue > 100)
            throw new Exception("Percentage discount cannot be greater than 100.");

        if (request.UsageLimit <= 0)
            throw new Exception("Usage limit must be greater than zero.");

        if (request.UsageLimit < coupon.UsedCount)
            throw new Exception("Usage limit cannot be lower than used count.");

        if (request.ExpireDate <= DateTime.UtcNow)
            throw new Exception("Expire date must be in the future.");

        var existingCoupon = await _context.GetCouponByCodeAsync(code, cancellationToken);

        if (existingCoupon is not null && existingCoupon.Id != coupon.Id)
            throw new Exception("Coupon code already exists.");

        coupon.Code = code;
        coupon.DiscountType = request.DiscountType;
        coupon.DiscountValue = request.DiscountValue;
        coupon.ExpireDate = request.ExpireDate;
        coupon.UsageLimit = request.UsageLimit;
        coupon.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}