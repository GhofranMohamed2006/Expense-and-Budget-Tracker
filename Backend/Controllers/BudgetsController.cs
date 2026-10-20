using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pennywise.Api.Data;
using PennywiseApi.Models;

namespace PennywiseApi.Controllers;

public record ExpenseDto(decimal Amount);

[ApiController]
[Route("api/budgets")]
public class BudgetsController : ControllerBase
{
    private readonly AppDbContext _db;
    public BudgetsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<BudgetCategory>> GetAll() =>
        await _db.BudgetCategories.ToListAsync();

    [HttpPost]
    public async Task<ActionResult<BudgetCategory>> Create(BudgetCategory c)
    {
        if (string.IsNullOrWhiteSpace(c.Name) || c.Limit <= 0)
            return BadRequest("Name and a positive limit are required.");
        c.Id = 0;
        c.Spent = 0;
        _db.BudgetCategories.Add(c);
        await _db.SaveChangesAsync();
        return Ok(c);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.BudgetCategories.FindAsync(id);
        if (c is null) return NotFound();
        _db.BudgetCategories.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/expenses")]
    public async Task<ActionResult<BudgetCategory>> AddExpense(int id, ExpenseDto dto)
    {
        if (dto.Amount <= 0) return BadRequest("Amount must be positive.");
        var c = await _db.BudgetCategories.FindAsync(id);
        if (c is null) return NotFound();
        c.Spent += dto.Amount;
        await _db.SaveChangesAsync();
        return Ok(c);
    }
}