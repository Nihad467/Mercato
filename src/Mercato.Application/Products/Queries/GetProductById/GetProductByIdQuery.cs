using MediatR;
using Mercato.Application.Product.DTOs;

namespace Mercato.Application.Product.Queries.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<GetProductByIdDto?>;