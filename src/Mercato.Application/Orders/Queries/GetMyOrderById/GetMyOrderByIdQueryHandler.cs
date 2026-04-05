using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Orders.DTOs;

namespace Mercato.Application.Orders.Queries.GetMyOrderById;

public class GetMyOrderByIdQueryHandler : IRequestHandler<GetMyOrderByIdQuery, OrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyOrderByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<OrderDto> Handle(GetMyOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var order = await _context.GetUserOrderByIdAsync(request.Id, userId, cancellationToken);
        if (order is null)
            throw new Exception("Order not found.");

        return new OrderDto
        {
            Id = order.Id,
            TotalPrice = order.TotalPrice,
            Status = order.Status.ToString(),
            CreatedAtUtc = order.CreatedAtUtc,
            Items = order.OrderItems.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }
}