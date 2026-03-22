using MediatR;
using Mercato.Application.Category.DTOs;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Category.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, GetCategoryByIdDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetCategoryByIdDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.GetCategoryByIdAsync(request.Id, cancellationToken);

        if (category is null)
            return null;

        return new GetCategoryByIdDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }
}