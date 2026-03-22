using FluentValidation;

namespace Mercato.Application.Category.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Category.Name)
            .NotEmpty().WithMessage("Category name bos ola bilmez.")
            .MinimumLength(2).WithMessage("Category name minimum 2 simvol olmalidir.")
            .MaximumLength(100).WithMessage("Category name maksimum 100 simvol ola biler.");
    }
}