using MediatR;

namespace Mercato.Application.Coupons.Queries.GetAllCoupons;

public class GetAllCouponsQuery : IRequest<List<GetAllCouponsResult>>
{
}