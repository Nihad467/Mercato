using MediatR;
using Mercato.Application.Category.DTOs;

namespace Mercato.Application.Category.Queries.GetAllCategories;

public record GetAllCategoriesQuery : IRequest<List<GetAllCategoriesDto>>;