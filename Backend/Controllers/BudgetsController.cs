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
[Route("api/budgets")]
public sealed class BudgetsController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BudgetDto>>> GetAll()
    {
        var budgets = await db.BudgetCategories.AsNoTracking()
            .Where(b => b.UserId == UserId)
            .OrderBy(b => b.Name)
            .ToListAsync();
        var categoryIds = budgets.Select(b => b.CategoryId).ToArray();
        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var end = start.AddMonths(1);
        var expenses = await db.Transactions.AsNoTracking()
            .Where(t => t.UserId == UserId && categoryIds.Contains(t.CategoryId)
                && t.Date >= start && t.Date < end)
            .Select(t => new { t.CategoryId, t.Amount })
            .ToListAsync();
        var spentByCategory = expenses
            .GroupBy(t => t.CategoryId)
            .ToDictionary(group => group.Key, group => group.Sum(t => t.Amount));

        return Ok(budgets.Select(b => new BudgetDto(
            b.Id, b.CategoryId, b.Name, b.Limit,
            spentByCategory.GetValueOrDefault(b.CategoryId))));
    }

    [HttpPost]
    public async Task<ActionResult<BudgetDto>> Create(BudgetRequest request)
    {
        var category = await FindExpenseCategory(request.CategoryId);
        if (category is null)
            return BadRequest(new { title = "Choose an expense category belonging to this account." });
        if (await db.BudgetCategories.AnyAsync(
                b => b.UserId == UserId && b.CategoryId == category.Id))
            return Problem(title: "A budget already exists for this category.", statusCode: 409);

        var budget = new BudgetCategory
        {
            UserId = UserId,
            CategoryId = category.Id,
            Name = category.Name,
            Limit = request.Limit
        };
        db.BudgetCategories.Add(budget);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), null,
            new BudgetDto(budget.Id, budget.CategoryId, budget.Name, budget.Limit, 0));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BudgetDto>> Update(int id, BudgetRequest request)
    {
        var budget = await db.BudgetCategories.FirstOrDefaultAsync(
            b => b.Id == id && b.UserId == UserId);
        if (budget is null)
            return NotFound();
        var category = await FindExpenseCategory(request.CategoryId);
        if (category is null)
            return BadRequest(new { title = "Choose an expense category belonging to this account." });
        if (await db.BudgetCategories.AnyAsync(
                b => b.UserId == UserId && b.CategoryId == category.Id && b.Id != id))
            return Problem(title: "A budget already exists for this category.", statusCode: 409);

        budget.CategoryId = category.Id;
        budget.Name = category.Name;
        budget.Limit = request.Limit;
        await db.SaveChangesAsync();
        return Ok(new BudgetDto(budget.Id, budget.CategoryId, budget.Name, budget.Limit, 0));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var budget = await db.BudgetCategories.FirstOrDefaultAsync(
            b => b.Id == id && b.UserId == UserId);
        if (budget is null)
            return NotFound();
        db.BudgetCategories.Remove(budget);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/expenses")]
    public async Task<ActionResult<TransactionDto>> AddExpense(
        int id, AmountRequest request)
    {
        var budget = await db.BudgetCategories.FirstOrDefaultAsync(
            b => b.Id == id && b.UserId == UserId);
        if (budget is null)
            return NotFound();

        var transaction = new Transaction
        {
            UserId = UserId,
            CategoryId = budget.CategoryId,
            Amount = request.Amount,
            Date = DateTime.UtcNow,
            Note = "Budget expense"
        };
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();
        return Ok(new TransactionDto(
            transaction.Id, budget.CategoryId, budget.Name, "expense",
            transaction.Amount, transaction.Date, transaction.Note));
    }

    private Task<Category?> FindExpenseCategory(int id) =>
        db.Categories.FirstOrDefaultAsync(
            c => c.Id == id && c.UserId == UserId && c.Type == CategoryType.Expense);
}
