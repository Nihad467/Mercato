using MediatR;
using Mercato.Application.Product.DTOs;

namespace Mercato.Application.Product.Commands.CreateProduct;

public record CreateProductCommand(CreateProductDto Product) : IRequest<int>;