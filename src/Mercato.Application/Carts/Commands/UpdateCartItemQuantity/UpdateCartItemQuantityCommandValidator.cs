using FluentValidation;

namespace Mercato.Application.Carts.Commands.UpdateCartItemQuantity;

public class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.CartItemId)
            .GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}