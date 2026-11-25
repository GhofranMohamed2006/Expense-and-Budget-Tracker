using Microsoft.EntityFrameworkCore;
using Pennywise.DAL.Context;
using Pennywise.DAL.Entities;

namespace Pennywise.DAL.Repositories;

public class CategoryRepository(AppDbContext context) : GenericRepository<Category>(context), ICategoryRepository
{
    public async Task<List<CategoryWithCounts>> GetByUserAsync(int userId, CategoryType? type)
    {
        var rows = await Set.AsNoTracking()
            .Where(c => c.UserId == userId && (type == null || c.Type == type))
            .OrderBy(c => c.Name)
            .Select(c => new { Category = c, Tx = c.Transactions.Count, Bg = c.Budgets.Count })
            .ToListAsync();
        return rows.Select(r => new CategoryWithCounts(r.Category, r.Tx, r.Bg)).ToList();
    }

    public async Task<CategoryWithCounts?> GetOwnedWithCountsAsync(int id, int userId)
    {
        var r = await Set.AsNoTracking()
            .Where(c => c.Id == id && c.UserId == userId)
            .Select(c => new { Category = c, Tx = c.Transactions.Count, Bg = c.Budgets.Count })
            .FirstOrDefaultAsync();
        return r is null ? null : new CategoryWithCounts(r.Category, r.Tx, r.Bg);
    }

    public Task<Category?> GetOwnedAsync(int id, int userId) =>
        Set.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

    public Task<bool> NameExistsAsync(int userId, CategoryType type, string name, int? exceptId = null)
    {
        var n = name.Trim().ToLower();
        return Set.AnyAsync(c => c.UserId == userId && c.Type == type
            && c.Name.ToLower() == n && (exceptId == null || c.Id != exceptId));
    }

    public async Task<CategoryUsage> GetUsageAsync(int id)
    {
        var tx = await Context.Transactions.CountAsync(t => t.CategoryId == id);
        var bg = await Context.Budgets.CountAsync(b => b.CategoryId == id);
        return new CategoryUsage(tx, bg);
    }
}
