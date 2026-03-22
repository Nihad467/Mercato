using FluentValidation;

namespace Mercato.Application.Product.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Product.Id)
            .GreaterThan(0).WithMessage("Id 0-dan boyuk olmalidir.");

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