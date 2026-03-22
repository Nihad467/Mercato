using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Category.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.GetCategoryByIdAsync(request.Id, cancellationToken);

        if (category is null)
            return false;

        _context.RemoveCategory(category);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}