using PennywiseApi.Models;

namespace Pennywise.Api.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public UserPreferences Preferences { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<BudgetCategory> Budgets { get; set; } = new();
    public List<Goal> Goals { get; set; } = new();
}

public class UserPreferences
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Currency { get; set; } = "EGP";
    public string Language { get; set; } = "English";
    public string Theme { get; set; } = "Light";
    public string DateFormat { get; set; } = "DD/MM/YYYY";
    public bool EmailNotifications { get; set; } = true;
    public bool BudgetAlerts { get; set; } = true;
    public bool MonthlySummary { get; set; } = false;
}

public enum CategoryType { Expense, Income }

public class Category
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = "";
    public CategoryType Type { get; set; }
    public string Icon { get; set; } = "tags";
    public int ColorIndex { get; set; }
    public List<Transaction> Transactions { get; set; } = new();
    public List<BudgetCategory> Budgets { get; set; } = new();
}

public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
}

public class RevokedToken
{
    public int Id { get; set; }
    public string TokenId { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}
