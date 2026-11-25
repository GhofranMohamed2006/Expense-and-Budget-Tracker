using Pennywise.BLL.DTOs;
using Pennywise.BLL.Exceptions;
using Pennywise.DAL.Entities;
using Pennywise.DAL.Repositories;

namespace Pennywise.BLL.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(int userId, CategoryType? type);
    Task<CategoryDto> GetByIdAsync(int userId, int id);
    Task<CategoryDto> CreateAsync(int userId, CategorySaveDto dto);
    Task<CategoryDto> UpdateAsync(int userId, int id, CategorySaveDto dto);
    Task DeleteAsync(int userId, int id);
}

public class CategoryService(IUnitOfWork uow) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(int userId, CategoryType? type)
    {
        var rows = await uow.Categories.GetByUserAsync(userId, type);
        return rows.Select(ToDto).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(int userId, int id)
    {
        var row = await uow.Categories.GetOwnedWithCountsAsync(id, userId)
                  ?? throw new NotFoundException("Category not found.");
        return ToDto(row);
    }

    public async Task<CategoryDto> CreateAsync(int userId, CategorySaveDto dto)
    {
        var name = dto.Name.Trim();
        if (name.Length == 0) throw new BusinessRuleException("Category name is required.");
        if (await uow.Categories.NameExistsAsync(userId, dto.Type, name))
            throw new ConflictException("A category with this name already exists.");

        var category = new Category { UserId = userId, Name = name, Type = dto.Type, IsDefault = false };
        await uow.Categories.AddAsync(category);
        await uow.SaveChangesAsync();
        return ToDto(new CategoryWithCounts(category, 0, 0));
    }

    public async Task<CategoryDto> UpdateAsync(int userId, int id, CategorySaveDto dto)
    {
        var category = await uow.Categories.GetOwnedAsync(id, userId)
                       ?? throw new NotFoundException("Category not found.");
        var name = dto.Name.Trim();
        if (name.Length == 0) throw new BusinessRuleException("Category name is required.");
        if (await uow.Categories.NameExistsAsync(userId, dto.Type, name, id))
            throw new ConflictException("A category with this name already exists.");

        var usage = await uow.Categories.GetUsageAsync(id);
        if (dto.Type != category.Type && (usage.TransactionCount > 0 || usage.BudgetCount > 0))
            throw new ConflictException("Cannot change the type of a category that has transactions or budgets.");

        category.Name = name;
        category.Type = dto.Type;
        await uow.SaveChangesAsync();
        return ToDto(new CategoryWithCounts(category, usage.TransactionCount, usage.BudgetCount));
    }

    public async Task DeleteAsync(int userId, int id)
    {
        var category = await uow.Categories.GetOwnedAsync(id, userId)
                       ?? throw new NotFoundException("Category not found.");
        if (category.IsDefault)
            throw new BusinessRuleException("Default categories cannot be deleted.");

        var usage = await uow.Categories.GetUsageAsync(id);
        if (usage.TransactionCount > 0)
            throw new ConflictException($"This category has {usage.TransactionCount} transactions. Move or delete them first.");
        if (usage.BudgetCount > 0)
            throw new ConflictException($"This category is used by {usage.BudgetCount} budgets. Remove them first.");

        uow.Categories.Remove(category);
        await uow.SaveChangesAsync();
    }

    private static CategoryDto ToDto(CategoryWithCounts r) => new(
        r.Category.Id, r.Category.Name, r.Category.Type, r.Category.IsDefault,
        r.Category.CreatedAt, r.TransactionCount, r.BudgetCount);
}
