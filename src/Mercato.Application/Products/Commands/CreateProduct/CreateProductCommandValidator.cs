using FluentValidation;
using Mercato.Application.Product.Commands.CreateProduct;

namespace Mercato.Application.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Product.Name)
            .NotEmpty().WithMessage("Product name bos ola bilmez.")
            .MinimumLength(2).WithMessage("Product name minimum 2 simvol olmalidir.")
            .MaximumLength(150).WithMessage("Product name maksimum 150 simvol ola biler.");

        RuleFor(x => x.Product.Description)
            .NotEmpty().WithMessage("Description bos ola bilmez.")
            .MaximumLength(500).WithMessage("Description maksimum 500 simvol ola biler.");

        RuleFor(x => x.Product.Price)
            .GreaterThan(0).WithMessage("Price 0-dan boyuk olmalidir.");

        RuleFor(x => x.Product.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock menfi ola bilmez.");

        RuleFor(x => x.Product.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId 0-dan boyuk olmalidir.");
    }
}