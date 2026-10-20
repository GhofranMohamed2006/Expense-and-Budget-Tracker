using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pennywise.Api.Data;
using Pennywise.Api.Models;

namespace Pennywise.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MonthlyReportDto>>> Get()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-11);
        var rows = await db.Transactions.AsNoTracking()
            .Where(t => t.UserId == userId && t.Date >= start)
            .Join(db.Categories.AsNoTracking(),
                transaction => new { transaction.CategoryId, transaction.UserId },
                category => new { CategoryId = category.Id, category.UserId },
                (transaction, category) => new
                {
                    transaction.Date,
                    transaction.Amount,
                    category.Type
                })
            .ToListAsync();

        var reports = Enumerable.Range(0, 12)
            .Select(offset => start.AddMonths(offset))
            .Select(month =>
            {
                var monthly = rows.Where(row =>
                    row.Date.Year == month.Year && row.Date.Month == month.Month);
                var income = monthly.Where(row => row.Type == CategoryType.Income)
                    .Sum(row => row.Amount);
                var expenses = monthly.Where(row => row.Type == CategoryType.Expense)
                    .Sum(row => row.Amount);
                return new MonthlyReportDto(month.Year, month.Month, income, expenses, income - expenses);
            })
            .ToList();

        return Ok(reports);
    }
}
