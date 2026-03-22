using MediatR;

namespace Mercato.Application.Category.Commands.DeleteCategory;

public record DeleteCategoryCommand(int Id) : IRequest<bool>;