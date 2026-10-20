using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pennywise.Api.Data;
using Pennywise.Api.Models;

namespace Pennywise.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public class CategoriesController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static CategoryType ParseType(string s) => Enum.Parse<CategoryType>(s, ignoreCase: true);

    private static CategoryDto ToDto(Category c, int count) =>
        new(c.Id, c.Name, c.Type.ToString().ToLowerInvariant(), c.Icon, c.ColorIndex, count);

    // GET api/categories             -> all
    // GET api/categories?type=income -> only income (or expense)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> List([FromQuery] string? type)
    {
        var q = db.Categories.AsNoTracking().Where(c => c.UserId == UserId);
        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!Enum.TryParse<CategoryType>(type, true, out var t))
                return Problem(title: "type must be 'expense' or 'income'.", statusCode: 400);
            q = q.Where(c => c.Type == t);
        }
        var rows = await q.OrderBy(c => c.Name)
            .Select(c => new { Cat = c, Count = c.Transactions.Count })
            .ToListAsync();
        return Ok(rows.Select(r => ToDto(r.Cat, r.Count)));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryRequest req)
    {
        var type = ParseType(req.Type);
        var name = req.Name.Trim();
        if (await Exists(name, type, null))
            return Problem(title: "A category with this name already exists.", statusCode: 409);

        var cat = new Category { UserId = UserId, Name = name, Type = type, Icon = req.Icon, ColorIndex = req.ColorIndex };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(List), new { type = req.Type }, ToDto(cat, 0));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, CategoryRequest req)
    {
        var cat = await db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
        if (cat is null) return NotFound();

        var type = ParseType(req.Type);
        var name = req.Name.Trim();
        if (await Exists(name, type, id))
            return Problem(title: "A category with this name already exists.", statusCode: 409);

        var count = await db.Transactions.CountAsync(t => t.CategoryId == id);
        if (type != cat.Type && count > 0)
            return Problem(title: "Cannot change the type of a category that has transactions.", statusCode: 409);

        cat.Name = name; cat.Type = type; cat.Icon = req.Icon; cat.ColorIndex = req.ColorIndex;
        await db.SaveChangesAsync();
        return Ok(ToDto(cat, count));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
        if (cat is null) return NotFound();

        var count = await db.Transactions.CountAsync(t => t.CategoryId == id);
        if (count > 0)
            return Problem(title: $"This category has {count} transactions. Move or delete them first.", statusCode: 409);

        db.Categories.Remove(cat);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private Task<bool> Exists(string name, CategoryType type, int? exceptId) =>
        db.Categories.AnyAsync(c => c.UserId == UserId && c.Type == type
            && c.Name.ToLower() == name.ToLower() && c.Id != exceptId);
}
