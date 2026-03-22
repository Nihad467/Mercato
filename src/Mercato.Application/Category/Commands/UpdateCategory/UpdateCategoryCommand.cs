using MediatR;
using Mercato.Application.Category.DTOs;

namespace Mercato.Application.Category.Commands.UpdateCategory;

public record UpdateCategoryCommand(UpdateCategoryDto Category) : IRequest<bool>;