using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PennywiseApi.Data;
using PennywiseApi.Models;

namespace PennywiseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(AppDbContext db) : ControllerBase
{
    // Everything is summed in memory: SQLite cannot SUM decimal columns on the server.
    private sealed record Row(DateOnly Date, TransactionType Type, string Category, decimal Amount);

    private async Task<List<Row>> LoadRows(DateOnly? from, DateOnly? to)
    {
        var query = db.Transactions.AsNoTracking();
        if (from is not null) query = query.Where(t => t.Date >= from);
        if (to is not null) query = query.Where(t => t.Date <= to);

        return await query
            .Select(t => new Row(t.Date, t.Type, t.Category, t.Amount))
            .ToListAsync();
    }

    private static PeriodTotals Totals(IEnumerable<Row> rows)
    {
        var list = rows.ToList();
        var income = list.Where(r => r.Type == TransactionType.Income).Sum(r => r.Amount);
        var expenses = list.Where(r => r.Type == TransactionType.Expense).Sum(r => r.Amount);
        return new PeriodTotals(income, expenses, income - expenses);
    }

    private static decimal? ChangePercent(decimal current, decimal previous) =>
        previous == 0 ? null : Math.Round((current - previous) / previous * 100, 1);

    private static int CountDays(DateOnly? from, DateOnly? to, List<Row> rows)
    {
        if (rows.Count == 0 && (from is null || to is null)) return 0;
        var start = from ?? rows.Min(r => r.Date);
        var end = to ?? rows.Max(r => r.Date);
        return Math.Max(1, end.DayNumber - start.DayNumber + 1);
    }

    /// <summary>Totals, savings rate and average daily spend, with a comparison to the previous period.</summary>
    [HttpGet("overview")]
    public async Task<ActionResult<OverviewDto>> Overview(
        [FromQuery(Name = "from")] DateOnly? fromDate,
        [FromQuery(Name = "to")] DateOnly? toDate)
    {
        if (fromDate is not null && toDate is not null && fromDate > toDate)
        {
            ModelState.AddModelError("from", "'from' must be on or before 'to'.");
            return ValidationProblem(ModelState);
        }

        var rows = await LoadRows(fromDate, toDate);
        var current = Totals(rows);
        var days = CountDays(fromDate, toDate, rows);

        PeriodTotals? previous = null;
        if (fromDate is not null && toDate is not null)
        {
            var length = toDate.Value.DayNumber - fromDate.Value.DayNumber + 1;
            var previousTo = fromDate.Value.AddDays(-1);
            var previousFrom = previousTo.AddDays(-(length - 1));
            previous = Totals(await LoadRows(previousFrom, previousTo));
        }

        var saved = current.Income > 0
            ? (int)Math.Max(0m, Math.Round(current.Net / current.Income * 100))
            : 0;

        return new OverviewDto(
            current.Income,
            current.Expenses,
            current.Net,
            saved,
            rows.Count,
            days,
            days > 0 ? Math.Round(current.Expenses / days, 2) : 0m,
            previous,
            previous is null ? null : ChangePercent(current.Income, previous.Income),
            previous is null ? null : ChangePercent(current.Expenses, previous.Expenses));
    }

    /// <summary>Income and expenses per calendar month, oldest first. Months with no data are included as zeros.</summary>
    [HttpGet("monthly")]
    public async Task<ActionResult<List<MonthlyDto>>> Monthly(
        [FromQuery] int months = 6,
        [FromQuery(Name = "to")] DateOnly? toDate = null)
    {
        if (months < 1 || months > 24)
        {
            ModelState.AddModelError("months", "months must be between 1 and 24.");
            return ValidationProblem(ModelState);
        }

        var end = toDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var firstMonth = new DateOnly(end.Year, end.Month, 1).AddMonths(-(months - 1));
        var rows = await LoadRows(firstMonth, end);

        var result = new List<MonthlyDto>();
        for (var i = 0; i < months; i++)
        {
            var month = firstMonth.AddMonths(i);
            var inMonth = rows.Where(r => r.Date.Year == month.Year && r.Date.Month == month.Month);
            var totals = Totals(inMonth);
            result.Add(new MonthlyDto(
                month.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                totals.Income,
                totals.Expenses,
                totals.Net));
        }

        return result;
    }

    /// <summary>Totals per category, largest first. Defaults to expenses.</summary>
    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryTotalDto>>> Categories(
        [FromQuery(Name = "from")] DateOnly? fromDate,
        [FromQuery(Name = "to")] DateOnly? toDate,
        [FromQuery] TransactionType type = TransactionType.Expense)
    {
        if (fromDate is not null && toDate is not null && fromDate > toDate)
        {
            ModelState.AddModelError("from", "'from' must be on or before 'to'.");
            return ValidationProblem(ModelState);
        }

        var rows = (await LoadRows(fromDate, toDate)).Where(r => r.Type == type).ToList();
        var grand = rows.Sum(r => r.Amount);

        return rows
            .GroupBy(r => r.Category)
            .Select(g => new
            {
                Category = g.Key,
                Total = g.Sum(r => r.Amount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .Select(x => new CategoryTotalDto(
                x.Category,
                x.Total,
                x.Count,
                grand > 0 ? Math.Round(x.Total / grand * 100, 1) : 0m))
            .ToList();
    }
}
