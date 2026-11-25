using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pennywise.BLL.DTOs;
using Pennywise.BLL.Services;
using Pennywise.DAL.Entities;

namespace Pennywise.API.Controllers;

[Authorize]
[Route("api/categories")]
public class CategoriesController(ICategoryService categories) : ApiControllerBase
{
    // GET api/categories  |  GET api/categories?type=expense|income
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll([FromQuery] CategoryType? type) =>
        Ok(await categories.GetAllAsync(UserId, type));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id) =>
        Ok(await categories.GetByIdAsync(UserId, id));

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategorySaveDto dto)
    {
        var created = await categories.CreateAsync(UserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, CategorySaveDto dto) =>
        Ok(await categories.UpdateAsync(UserId, id, dto));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await categories.DeleteAsync(UserId, id);
        return NoContent();
    }
}
