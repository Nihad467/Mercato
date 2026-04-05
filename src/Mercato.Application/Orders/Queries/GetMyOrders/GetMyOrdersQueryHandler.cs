using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Orders.DTOs;

namespace Mercato.Application.Orders.Queries.GetMyOrders;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, List<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyOrdersQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var orders = await _context.GetUserOrdersAsync(userId, cancellationToken);

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