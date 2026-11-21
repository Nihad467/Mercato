using MediatR;
using Mercato.Application.Common.Models.Pagination;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Product.Commands.CreateProduct;
using Mercato.Application.Product.Commands.UpdateProduct;
using Mercato.Application.Product.DTOs;
using Mercato.Application.Product.Queries.GetProductById;
using Mercato.Application.Products.Commands.CreateProduct;
using Mercato.Application.Products.Commands.DeleteProduct;
using Mercato.Application.Products.Queries.GetAllProducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Minio.Exceptions;

namespace Mercato.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public ProductsController(
        IMediator mediator,
        IApplicationDbContext context,
        IFileStorageService fileStorage)
    {
        _mediator = mediator;
        _context = context;
        _fileStorage = fileStorage;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
    {
        var command = new CreateProductCommand(dto);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDto dto)
    {
        dto.Id = id;

        var command = new UpdateProductCommand(dto);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = "Product tapılmadı" });

        return NoContent();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameters parameters)
    {
        var query = new GetAllProductsQuery
        {
            Parameters = parameters
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));

        if (result is null)
            return NotFound(new { message = "Product tapılmadı" });

        return Ok(result);
    }

    [HttpGet("{id}/image")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMainImage(int id, CancellationToken cancellationToken)
    {
        var product = await _context.GetProductByIdAsync(id, cancellationToken);

        if (product is null)
            return NotFound(new { message = "Product tapılmadı" });

        var image = product.Images
            .OrderByDescending(item => item.IsMain)
            .ThenBy(item => item.Order)
            .FirstOrDefault();

        if (image is null)
            return NotFound(new { message = "Product image tapılmadı" });

        Stream stream;
        try
        {
            stream = await _fileStorage.OpenReadAsync(image.ObjectKey, cancellationToken);
        }
        catch (ObjectNotFoundException)
        {
            return NotFound(new { message = "Product image faylı MinIO-da tapılmadı" });
        }

        var contentTypeProvider = new FileExtensionContentTypeProvider();

        if (!contentTypeProvider.TryGetContentType(image.ObjectKey, out var contentType))
            contentType = "application/octet-stream";

        return File(stream, contentType);
    }
}
