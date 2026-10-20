using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PennywiseApi.Data;
using PennywiseApi.Models;

namespace PennywiseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController(AppDbContext db) : ControllerBase
{
    private static TransactionDto ToDto(Transaction t) =>
        new(t.Id, t.Description, t.Amount, t.Type, t.Category, t.Account, t.Date);

    private IQueryable<Transaction> Filter(TransactionQuery f)
    {
        var query = db.Transactions.AsNoTracking();

        if (f.Type is not null) query = query.Where(t => t.Type == f.Type);
        if (!string.IsNullOrWhiteSpace(f.Category)) query = query.Where(t => t.Category == f.Category);
        if (!string.IsNullOrWhiteSpace(f.Account)) query = query.Where(t => t.Account == f.Account);
        if (f.From is not null) query = query.Where(t => t.Date >= f.From);
        if (f.To is not null) query = query.Where(t => t.Date <= f.To);

        var text = !string.IsNullOrWhiteSpace(f.Search) ? f.Search : f.Q;
        if (!string.IsNullOrWhiteSpace(text))
        {
            var term = text.Trim();
            query = query.Where(t => EF.Functions.Like(t.Description, $"%{term}%"));
        }

        return query;
    }

    /// <summary>Checks the category and account against the allowed lists.</summary>
    private bool ValidateLookups(TransactionRequest r)
    {
        if (!Lookups.Categories.Contains(r.Category))
            ModelState.AddModelError(nameof(r.Category), $"Category must be one of: {string.Join(", ", Lookups.Categories)}.");
        if (!Lookups.Accounts.Contains(r.Account))
            ModelState.AddModelError(nameof(r.Account), $"Account must be one of: {string.Join(", ", Lookups.Accounts)}.");
        return ModelState.IsValid;
    }

    /// <summary>List transactions, newest first. All filters are optional.</summary>
    [HttpGet]
    public async Task<ActionResult<List<TransactionDto>>> GetAll([FromQuery] TransactionQuery filter)
    {
        var items = await Filter(filter)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        return items.Select(ToDto).ToList();
    }

    /// <summary>Income, expenses and net for the same filters as the list.</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<SummaryDto>> GetSummary([FromQuery] TransactionQuery filter)
    {
        // Summed in memory: SQLite cannot SUM decimal columns on the server.
        var rows = await Filter(filter).Select(t => new { t.Type, t.Amount }).ToListAsync();

        var income = rows.Where(r => r.Type == TransactionType.Income).Sum(r => r.Amount);
        var expenses = rows.Where(r => r.Type == TransactionType.Expense).Sum(r => r.Amount);
        var net = income - expenses;
        var saved = income > 0 ? (int)Math.Max(0, Math.Round(net / income * 100)) : 0;

        return new SummaryDto(
            income, expenses, net,
            rows.Count(r => r.Type == TransactionType.Income),
            rows.Count(r => r.Type == TransactionType.Expense),
            saved);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransactionDto>> GetById(int id)
    {
        var t = await db.Transactions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return t is null ? NotFound() : ToDto(t);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create(TransactionRequest request)
    {
        if (!ValidateLookups(request)) return ValidationProblem(ModelState);

        var t = new Transaction
        {
            Description = request.Description.Trim(),
            Amount = request.Amount,
            Type = request.Type!.Value,
            Category = request.Category,
            Account = request.Account,
            Date = request.Date!.Value
        };

        db.Transactions.Add(t);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = t.Id }, ToDto(t));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TransactionDto>> Update(int id, TransactionRequest request)
    {
        if (!ValidateLookups(request)) return ValidationProblem(ModelState);

        var t = await db.Transactions.FindAsync(id);
        if (t is null) return NotFound();

        t.Description = request.Description.Trim();
        t.Amount = request.Amount;
        t.Type = request.Type!.Value;
        t.Category = request.Category;
        t.Account = request.Account;
        t.Date = request.Date!.Value;

        await db.SaveChangesAsync();
        return ToDto(t);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await db.Transactions.FindAsync(id);
        if (t is null) return NotFound();

        db.Transactions.Remove(t);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
