using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PennywiseApi.Data;
using PennywiseApi.Models;

namespace PennywiseApi.Controllers;

public record ContributionDto(decimal Amount);

[ApiController]
[Route("api/goals")]
public class GoalsController : ControllerBase
{
    private readonly AppDbContext _db;
    public GoalsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<Goal>> GetAll() =>
        await _db.Goals.ToListAsync();

    [HttpPost]
    public async Task<ActionResult<Goal>> Create(Goal g)
    {
        if (string.IsNullOrWhiteSpace(g.Name) || g.Target <= 0)
            return BadRequest("Name and a positive target are required.");
        g.Id = 0;
        g.Saved = 0;
        _db.Goals.Add(g);
        await _db.SaveChangesAsync();
        return Ok(g);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var g = await _db.Goals.FindAsync(id);
        if (g is null) return NotFound();
        _db.Goals.Remove(g);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/contributions")]
    public async Task<ActionResult<Goal>> AddContribution(int id, ContributionDto dto)
    {
        if (dto.Amount <= 0) return BadRequest("Amount must be positive.");
        var g = await _db.Goals.FindAsync(id);
        if (g is null) return NotFound();
        g.Saved += dto.Amount;
        await _db.SaveChangesAsync();
        return Ok(g);
    }
}