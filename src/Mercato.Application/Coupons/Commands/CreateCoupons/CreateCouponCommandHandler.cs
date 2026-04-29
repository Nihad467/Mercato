using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Domain.Entities;
using Mercato.Domain.Enums;

namespace Mercato.Application.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, CreateCouponResult>
{
    private readonly IApplicationDbContext _context;

    public CreateCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateCouponResult> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(code))
            throw new Exception("Coupon code is required.");

        var codeExists = await _context.CouponCodeExistsAsync(code, cancellationToken);

        if (codeExists)
            throw new Exception("Coupon code already exists.");

        if (request.DiscountValue <= 0)
            throw new Exception("Discount value must be greater than zero.");

        if (request.DiscountType == DiscountType.Percentage && request.DiscountValue > 100)
            throw new Exception("Percentage discount cannot be greater than 100.");

        if (request.UsageLimit <= 0)
            throw new Exception("Usage limit must be greater than zero.");

        if (request.ExpireDate <= DateTime.UtcNow)
            throw new Exception("Expire date must be in the future.");

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = code,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            ExpireDate = request.ExpireDate,
            UsageLimit = request.UsageLimit,
            UsedCount = 0,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _context.AddCouponAsync(coupon, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCouponResult
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