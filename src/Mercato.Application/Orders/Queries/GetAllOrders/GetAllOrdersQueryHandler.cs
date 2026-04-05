using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Orders.DTOs;

namespace Mercato.Application.Orders.Queries.GetAllOrders;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, List<OrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.GetAllOrdersAsync(cancellationToken);

        return orders.Select(x => new OrderDto
        {
            Id = x.Id,
            TotalPrice = x.TotalPrice,
            Status = x.Status.ToString(),
            CreatedAtUtc = x.CreatedAtUtc,
            Items = x.OrderItems.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice
            }).ToList()
        }).ToList();
    }
}