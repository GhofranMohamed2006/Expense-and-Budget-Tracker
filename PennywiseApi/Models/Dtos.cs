using System.ComponentModel.DataAnnotations;

namespace PennywiseApi.Models;

/// <summary>Body for POST and PUT.</summary>
public class TransactionRequest
{
    [Required, StringLength(120, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 1_000_000_000, ErrorMessage = "Amount must be above zero.")]
    public decimal Amount { get; set; }

    [Required]
    public TransactionType? Type { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string Account { get; set; } = "Main account";

    [Required]
    public DateOnly? Date { get; set; }
}

/// <summary>Query string filters for the list and the summary.</summary>
public class TransactionQuery
{
    public TransactionType? Type { get; set; }
    public string? Category { get; set; }
    public string? Account { get; set; }

    /// <summary>Search text matched against the description.</summary>
    public string? Search { get; set; }

    /// <summary>Short alias of <c>search</c>.</summary>
    public string? Q { get; set; }

    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}

public record TransactionDto(
    int Id,
    string Description,
    decimal Amount,
    TransactionType Type,
    string Category,
    string Account,
    DateOnly Date);

public record SummaryDto(
    decimal Income,
    decimal Expenses,
    decimal Net,
    int IncomeCount,
    int ExpenseCount,
    int SavedPercent);
