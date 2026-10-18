namespace PennywiseApi.Models;

public enum TransactionType
{
    Income,
    Expense
}

public class Transaction
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Allowed values, same lists the page shows in its dropdowns.</summary>
public static class Lookups
{
    public static readonly string[] Categories =
    {
        "Salary", "Freelance", "Groceries", "Transport", "Bills",
        "Dining", "Health", "Shopping", "Subscriptions"
    };

    public static readonly string[] Accounts = { "Main account", "Cash wallet", "Savings" };
}
