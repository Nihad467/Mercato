using FluentValidation;

namespace Mercato.Application.Product.Commands.UpdateProduct;

public class UpdateProductCommandValidator
    : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Product.Id)
            .NotEmpty();

        RuleFor(x => x.Product.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Product.Price)
            .GreaterThan(0);
    }
}