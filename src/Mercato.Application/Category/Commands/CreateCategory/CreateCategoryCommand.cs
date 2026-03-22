using MediatR;
using Mercato.Application.Category.DTOs;

namespace Mercato.Application.Category.Commands.CreateCategory;

public record CreateCategoryCommand(CreateCategoryDto Category) : IRequest<int>;