using MediatR;
using Mercato.Application.Category.DTOs;

namespace Mercato.Application.Category.Queries.GetCategoryById;

public record GetCategoryByIdQuery(int Id) : IRequest<GetCategoryByIdDto?>;