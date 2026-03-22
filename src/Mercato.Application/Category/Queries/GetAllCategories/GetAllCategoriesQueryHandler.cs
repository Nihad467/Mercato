using MediatR;
using Mercato.Application.Category.DTOs;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Category.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<GetAllCategoriesDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GetAllCategoriesDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.GetAllCategoriesAsync(cancellationToken);

        return categories.Select(category => new GetAllCategoriesDto
        {
            Id = category.Id,
            Name = category.Name
        }).ToList();
    }
}