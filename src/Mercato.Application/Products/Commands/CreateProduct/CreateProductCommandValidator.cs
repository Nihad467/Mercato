using FluentValidation;

namespace Mercato.Application.Product.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Product.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Product.Price)
            .GreaterThan(0);

        RuleFor(x => x.Product.Stock)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Product.CategoryId)
            .NotEmpty();
    }
}