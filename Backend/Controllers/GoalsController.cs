using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pennywise.Api.Data;
using Pennywise.Api.Models;
using PennywiseApi.Models;

namespace Pennywise.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/goals")]
public sealed class GoalsController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Goal>>> GetAll() =>
        Ok(await db.Goals.AsNoTracking()
            .Where(goal => goal.UserId == UserId)
            .OrderBy(goal => goal.Date)
            .ThenBy(goal => goal.Name)
            .ToListAsync());

    [HttpPost]
    public async Task<ActionResult<Goal>> Create(GoalRequest request)
    {
        var name = request.Name.Trim();
        if (await NameExists(name, null))
            return Problem(title: "A savings goal with this name already exists.", statusCode: 409);

        var goal = new Goal
        {
            UserId = UserId,
            Name = name,
            Target = request.Target,
            Date = request.Date,
            Saved = 0
        };
        db.Goals.Add(goal);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), null, goal);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Goal>> Update(int id, GoalRequest request)
    {
        var goal = await db.Goals.FirstOrDefaultAsync(
            item => item.Id == id && item.UserId == UserId);
        if (goal is null)
            return NotFound();
        var name = request.Name.Trim();
        if (await NameExists(name, id))
            return Problem(title: "A savings goal with this name already exists.", statusCode: 409);

        goal.Name = name;
        goal.Target = request.Target;
        goal.Date = request.Date;
        await db.SaveChangesAsync();
        return Ok(goal);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var goal = await db.Goals.FirstOrDefaultAsync(
            item => item.Id == id && item.UserId == UserId);
        if (goal is null)
            return NotFound();
        db.Goals.Remove(goal);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/contributions")]
    public async Task<ActionResult<Goal>> AddContribution(int id, AmountRequest request)
    {
        var goal = await db.Goals.FirstOrDefaultAsync(
            item => item.Id == id && item.UserId == UserId);
        if (goal is null)
            return NotFound();
        goal.Saved += request.Amount;
        await db.SaveChangesAsync();
        return Ok(goal);
    }

    private Task<bool> NameExists(string name, int? exceptId) =>
        db.Goals.AnyAsync(goal => goal.UserId == UserId
            && EF.Functions.Collate(goal.Name, "NOCASE") == name
            && goal.Id != exceptId);
}
