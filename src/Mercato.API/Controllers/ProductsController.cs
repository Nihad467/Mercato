using MediatR;
using Mercato.Application.Product.Commands.CreateProduct;
using Mercato.Application.Product.Commands.UpdateProduct;
using Mercato.Application.Product.DTOs;
using Mercato.Application.Products.Commands.CreateProduct;
using Mercato.Application.Products.Commands.DeleteProduct;
using Microsoft.AspNetCore.Mvc;
using Mercato.Application.Product.Queries.GetAllProducts;
using Mercato.Application.Product.Queries.GetProductById;
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
    public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
    {
        var command = new CreateProductCommand(dto);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDto dto)
    {
        dto.Id = id;
        var command = new UpdateProductCommand(dto);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = "Product tapılmadı" });

        return NoContent();
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));

        if (result is null)
            return NotFound(new { message = "Product tapılmadı" });

        return Ok(result);
    }
}