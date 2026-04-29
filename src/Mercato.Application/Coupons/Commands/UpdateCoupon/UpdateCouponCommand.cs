using MediatR;
using Mercato.Domain.Enums;

namespace Mercato.Application.Coupons.Commands.UpdateCoupon;

public class UpdateCouponCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public DateTime ExpireDate { get; set; }

    public int UsageLimit { get; set; }

    public bool IsActive { get; set; }
}