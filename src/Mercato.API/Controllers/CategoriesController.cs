using MediatR;
using Mercato.Application.Category.Commands.CreateCategory;
using Mercato.Application.Category.Commands.DeleteCategory;
using Mercato.Application.Category.Commands.UpdateCategory;
using Mercato.Application.Category.DTOs;
using Mercato.Application.Category.Queries.GetAllCategories;
using Mercato.Application.Category.Queries.GetCategoryById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mercato.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var command = new CreateCategoryCommand(dto);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        dto.Id = id;

        var command = new UpdateCategoryCommand(dto);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = "Category tapılmadı" });

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(id));

        if (!result.IsSuccess && result.Message == "Category tapılmadı.")
            return NotFound(new { message = result.Message });

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return NoContent();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllCategoriesQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id));

        if (result is null)
            return NotFound();

        return Ok(result);
    }
}