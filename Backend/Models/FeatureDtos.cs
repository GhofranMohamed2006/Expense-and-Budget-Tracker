using System.ComponentModel.DataAnnotations;

namespace Pennywise.Api.Models;

public sealed class BudgetRequest
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal Limit { get; set; }
}

public sealed record BudgetDto(
    int Id,
    int CategoryId,
    string Name,
    decimal Limit,
    decimal Spent);

public sealed class GoalRequest
{
    [Required, StringLength(80, MinimumLength = 1)]
    public string Name { get; set; } = "";

    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal Target { get; set; }

    public DateTime? Date { get; set; }
}

public sealed class AmountRequest
{
    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal Amount { get; set; }
}

public sealed record DashboardDto(
    decimal Income,
    decimal Expenses,
    decimal Balance,
    IReadOnlyList<TransactionDto> RecentTransactions,
    IReadOnlyList<BudgetDto> Budgets);

public sealed record MonthlyReportDto(
    int Year,
    int Month,
    decimal Income,
    decimal Expenses,
    decimal Balance);
