using MediatR;
using Mercato.Application.Category.Commands.CreateCategory;
using Mercato.Application.Category.Commands.DeleteCategory;
using Mercato.Application.Category.Commands.UpdateCategory;
using Mercato.Application.Category.DTOs;
using Mercato.Application.Category.Queries.GetAllCategories;
using Mercato.Application.Category.Queries.GetCategoryById;
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
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        var command = new CreateCategoryCommand(dto);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateCategoryDto dto)
    {
        var command = new UpdateCategoryCommand(dto);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = "Category tapılmadı" });

        return Ok(result);
    }

    [HttpDelete("{id}")]
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
            return NotFound(new { message = "Category tapılmadı" });

        return Ok(result);
    }
}