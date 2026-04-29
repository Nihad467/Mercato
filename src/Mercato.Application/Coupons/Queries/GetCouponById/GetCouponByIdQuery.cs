using MediatR;

namespace Mercato.Application.Coupons.Queries.GetCouponById;

public class GetCouponByIdQuery : IRequest<GetCouponByIdResult?>
{
    public Guid Id { get; set; }

    public GetCouponByIdQuery(Guid id)
    {
        Id = id;
    }
}