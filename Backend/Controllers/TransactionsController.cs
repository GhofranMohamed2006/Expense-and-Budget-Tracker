using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pennywise.Api.Data;
using Pennywise.Api.Models;

namespace Pennywise.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/transactions")]
public class TransactionsController(AppDbContext db) : ControllerBase
{
    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private IQueryable<Transaction> Filter(
        string? type, int? categoryId,
        DateTime? from, DateTime? to, string? search)
    {
        var q = db.Transactions.AsNoTracking()
            .Where(t => t.UserId == UserId &&
                db.Categories.Any(c =>
                    c.Id == t.CategoryId && c.UserId == UserId));

        if (!string.IsNullOrWhiteSpace(type))
        {
            var income = type.Equals("income", StringComparison.OrdinalIgnoreCase);
            q = q.Where(t => db.Categories.Any(c =>
                c.Id == t.CategoryId && c.UserId == UserId &&
                c.Type == (income ? CategoryType.Income : CategoryType.Expense)));
        }

        if (categoryId.HasValue)
            q = q.Where(t => t.CategoryId == categoryId.Value);

        if (from.HasValue)
            q = q.Where(t => t.Date >= from.Value.Date);

        if (to.HasValue)
            q = q.Where(t => t.Date < to.Value.Date.AddDays(1));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(t => (t.Note ?? "").Contains(term));
        }

        return q;
    }

    private IQueryable<TransactionDto> ToDtos(IQueryable<Transaction> q) =>
        q.Select(t => new TransactionDto(
            t.Id,
            t.CategoryId,
            db.Categories.Where(c => c.Id == t.CategoryId && c.UserId == UserId)
                .Select(c => c.Name).FirstOrDefault() ?? "",
            db.Categories.Where(c => c.Id == t.CategoryId && c.UserId == UserId)
                .Select(c => c.Type == CategoryType.Income ? "income" : "expense")
                .FirstOrDefault() ?? "expense",
            t.Amount,
            t.Date,
            t.Note));

    private async Task<Category?> FindCategory(int id) =>
        await db.Categories.FirstOrDefaultAsync(c =>
            c.Id == id && c.UserId == UserId);

    private static bool ValidType(string? type) =>
        string.Equals(type, "income", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(type, "expense", StringComparison.OrdinalIgnoreCase);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAll(
    [FromQuery] string? type,
    [FromQuery] int? categoryId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromQuery] string? search)
    {
        if (!string.IsNullOrWhiteSpace(type) && !ValidType(type))
        {
            return BadRequest(new
            {
                message = "Type must be income or expense."
            });
        }

        // Apply filters and ordering to database entities first.
        var entities = await Filter(type, categoryId, from, to, search)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        // Load the categories needed to build the response.
        var categoryIds = entities
            .Select(t => t.CategoryId)
            .Distinct()
            .ToList();

        var categories = await db.Categories
            .AsNoTracking()
            .Where(c => c.UserId == UserId && categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);

        // Build DTOs after retrieving the entities from the database.
        var result = entities.Select(t =>
        {
            var category = categories[t.CategoryId];

            return new TransactionDto(
                t.Id,
                t.CategoryId,
                category.Name,
                category.Type == CategoryType.Income ? "income" : "expense",
                t.Amount,
                t.Date,
                t.Note
            );
        }).ToList();

        return Ok(result);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransactionDto>> GetById(int id)
    {
        var query = db.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Id == id &&
                t.UserId == UserId &&
                db.Categories.Any(c =>
                    c.Id == t.CategoryId &&
                    c.UserId == UserId));

        var transaction = await ToDtos(query).FirstOrDefaultAsync();

        if (transaction is null)
            return NotFound();

        return Ok(transaction);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create(TransactionRequest req)
    {
        if (!ValidType(req.Type))
            return BadRequest(new { title = "Type must be income or expense." });

        var category = await FindCategory(req.CategoryId);
        if (category is null)
            return BadRequest(new { title = "Category not found for this account." });

        var expectedType = category.Type == CategoryType.Income ? "income" : "expense";
        if (!expectedType.Equals(req.Type, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { title = "The category type does not match the transaction type." });

        var transaction = new Transaction
        {
            UserId = UserId,
            CategoryId = category.Id,
            Amount = req.Amount,
            Date = req.Date,
            Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim()
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        var dto = await ToDtos(
            db.Transactions.AsNoTracking().Where(t => t.Id == transaction.Id))
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TransactionDto>> Update(
        int id, TransactionRequest req)
    {
        if (!ValidType(req.Type))
            return BadRequest(new { title = "Type must be income or expense." });

        var transaction = await db.Transactions.FirstOrDefaultAsync(t =>
            t.Id == id && t.UserId == UserId);

        if (transaction is null) return NotFound();

        var category = await FindCategory(req.CategoryId);
        if (category is null)
            return BadRequest(new { title = "Category not found for this account." });

        var expectedType = category.Type == CategoryType.Income ? "income" : "expense";
        if (!expectedType.Equals(req.Type, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { title = "The category type does not match the transaction type." });

        transaction.CategoryId = category.Id;
        transaction.Amount = req.Amount;
        transaction.Date = req.Date;
        transaction.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();

        await db.SaveChangesAsync();

        return Ok(await ToDtos(
            db.Transactions.AsNoTracking().Where(t => t.Id == id)).FirstAsync());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var transaction = await db.Transactions.FirstOrDefaultAsync(t =>
            t.Id == id && t.UserId == UserId);

        if (transaction is null) return NotFound();

        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
