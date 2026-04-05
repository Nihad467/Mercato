using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Domain.Entities;
using Mercato.Domain.Enums;

namespace Mercato.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var cartItems = await _context.GetUserCartItemsAsync(userId, cancellationToken);

        if (!cartItems.Any())
            throw new Exception("Cart is empty.");

        foreach (var cartItem in cartItems)
        {
            if (cartItem.Product is null)
                throw new Exception("Product not found in cart.");

            if (cartItem.Product.Stock < cartItem.Quantity)
                throw new Exception($"Not enough stock for product: {cartItem.Product.Name}");
        }

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var cartItem in cartItems)
        {
            var unitPrice = cartItem.Product.Price;
            var totalPrice = unitPrice * cartItem.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.Name,
                UnitPrice = unitPrice,
                Quantity = cartItem.Quantity,
                TotalPrice = totalPrice
            };

            order.OrderItems.Add(orderItem);

            cartItem.Product.Stock -= cartItem.Quantity;
            _context.RemoveCartItem(cartItem);
        }

        order.TotalPrice = order.OrderItems.Sum(x => x.TotalPrice);

        await _context.AddOrderAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}