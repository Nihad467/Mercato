using MediatR;
using Mercato.Application.Product.DTOs;

namespace Mercato.Application.Product.Commands.UpdateProduct;

public record UpdateProductCommand(UpdateProductDto Product) : IRequest<bool>;