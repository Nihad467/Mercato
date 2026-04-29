using MediatR;
using Mercato.Domain.Enums;

namespace Mercato.Application.Coupons.Commands.CreateCoupon;

public class CreateCouponCommand : IRequest<CreateCouponResult>
{
    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public DateTime ExpireDate { get; set; }

    public int UsageLimit { get; set; }

    public bool IsActive { get; set; } = true;
}