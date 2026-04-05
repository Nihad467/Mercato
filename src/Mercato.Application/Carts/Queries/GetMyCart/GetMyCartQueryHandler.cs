using MediatR;
using Mercato.Application.Carts.DTOs;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Carts.Queries.GetMyCart;

public class GetMyCartQueryHandler : IRequestHandler<GetMyCartQuery, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyCartQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CartDto> Handle(GetMyCartQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var cartItems = await _context.GetUserCartItemsAsync(userId, cancellationToken);

        var items = cartItems.Select(x =>
        {
            var mainImage = x.Product.Images
                .OrderBy(i => i.Order)
                .FirstOrDefault();

            return new CartItemDto
            {
                CartItemId = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                UnitPrice = x.Product.Price,
                Quantity = x.Quantity,
                TotalPrice = x.Product.Price * x.Quantity,
                MainImageObjectKey = mainImage?.ObjectKey
            };
        }).ToList();

        return new CartDto
        {
            Items = items,
            TotalPrice = items.Sum(x => x.TotalPrice)
        };
    }
}