using FluentValidation;

namespace Mercato.Application.Category.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Category.Id)
            .GreaterThan(0).WithMessage("Id 0-dan boyuk olmalidir.");

        RuleFor(x => x.Category.Name)
            .NotEmpty().WithMessage("Category name bos ola bilmez.")
            .MinimumLength(2).WithMessage("Category name minimum 2 simvol olmalidir.")
            .MaximumLength(100).WithMessage("Category name maksimum 100 simvol ola biler.");
    }
}