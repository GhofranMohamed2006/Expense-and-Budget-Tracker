using Pennywise.DAL.Entities;

namespace Pennywise.DAL.Repositories;

public record CategoryWithCounts(Category Category, int TransactionCount, int BudgetCount);
public record CategoryUsage(int TransactionCount, int BudgetCount);

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<List<CategoryWithCounts>> GetByUserAsync(int userId, CategoryType? type);
    Task<CategoryWithCounts?> GetOwnedWithCountsAsync(int id, int userId);
    Task<Category?> GetOwnedAsync(int id, int userId);   // tracked, for updates/deletes
    Task<bool> NameExistsAsync(int userId, CategoryType type, string name, int? exceptId = null);
    Task<CategoryUsage> GetUsageAsync(int id);
}
