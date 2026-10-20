using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pennywise.Api.Data;
using Pennywise.Api.Models;

namespace Pennywise.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var transactions = await db.Transactions.AsNoTracking()
            .Where(t => t.UserId == userId)
            .Join(db.Categories.AsNoTracking(),
                transaction => new { transaction.CategoryId, transaction.UserId },
                category => new { CategoryId = category.Id, category.UserId },
                (transaction, category) => new { transaction, category })
            .OrderByDescending(row => row.transaction.Date)
            .ThenByDescending(row => row.transaction.Id)
            .ToListAsync();

        var income = transactions
            .Where(row => row.category.Type == CategoryType.Income)
            .Sum(row => row.transaction.Amount);
        var expenses = transactions
            .Where(row => row.category.Type == CategoryType.Expense)
            .Sum(row => row.transaction.Amount);
        var recent = transactions.Take(8)
            .Select(row => new TransactionDto(
                row.transaction.Id,
                row.transaction.CategoryId,
                row.category.Name,
                row.category.Type == CategoryType.Income ? "income" : "expense",
                row.transaction.Amount,
                row.transaction.Date,
                row.transaction.Note))
            .ToList();

        var budgets = await db.BudgetCategories.AsNoTracking()
            .Where(budget => budget.UserId == userId)
            .OrderBy(budget => budget.Name)
            .ToListAsync();
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var nextMonth = monthStart.AddMonths(1);
        var monthExpenses = transactions
            .Where(row => row.category.Type == CategoryType.Expense
                && row.transaction.Date >= monthStart
                && row.transaction.Date < nextMonth)
            .GroupBy(row => row.transaction.CategoryId)
            .ToDictionary(group => group.Key, group => group.Sum(row => row.transaction.Amount));
        var budgetDtos = budgets.Select(budget => new BudgetDto(
            budget.Id,
            budget.CategoryId,
            budget.Name,
            budget.Limit,
            monthExpenses.GetValueOrDefault(budget.CategoryId)))
            .ToList();

        return Ok(new DashboardDto(
            income,
            expenses,
            income - expenses,
            recent,
            budgetDtos));
    }
}
