using System.ComponentModel.DataAnnotations;
using Pennywise.DAL.Entities;

namespace Pennywise.BLL.DTOs;

public class CategorySaveDto
{
    [Required, StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    public CategoryType Type { get; set; } = CategoryType.Expense;
}

public record CategoryDto(
    int Id,
    string Name,
    CategoryType Type,
    bool IsDefault,
    DateTime CreatedAt,
    int TransactionCount,
    int BudgetCount);
