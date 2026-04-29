using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Coupons.Queries.GetAllCoupons;

public class GetAllCouponsQueryHandler : IRequestHandler<GetAllCouponsQuery, List<GetAllCouponsResult>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCouponsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GetAllCouponsResult>> Handle(GetAllCouponsQuery request, CancellationToken cancellationToken)
    {
        var coupons = await _context.GetAllCouponsAsync(cancellationToken);

        return coupons.Select(c => new GetAllCouponsResult
        {
            Id = c.Id,
            Code = c.Code,
            DiscountType = c.DiscountType,
            DiscountValue = c.DiscountValue,
            ExpireDate = c.ExpireDate,
            UsageLimit = c.UsageLimit,
            UsedCount = c.UsedCount,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        }).ToList();
    }
}