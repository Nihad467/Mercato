using MediatR;
using Mercato.Application.Product.Commands.CreateProduct;
using Mercato.Application.Product.DTOs;
using Mercato.Application.Products.Commands.CreateProduct;
using Microsoft.AspNetCore.Mvc;

namespace Mercato.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        var command = new CreateProductCommand(dto);

        var result = await _mediator.Send(command);

        return Ok(result);
    }
}