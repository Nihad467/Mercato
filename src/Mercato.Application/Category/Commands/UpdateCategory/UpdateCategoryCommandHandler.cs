using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Category.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.GetCategoryByIdAsync(request.Category.Id, cancellationToken);

        if (category is null)
            return false;

        category.Name = request.Category.Name;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}