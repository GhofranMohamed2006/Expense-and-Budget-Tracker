using System.ComponentModel.DataAnnotations;

namespace Pennywise.Api.Models;

public class TransactionRequest
{
    [Required, RegularExpression("(?i)^(income|expense)$")]
    public string Type { get; set; } = "expense";

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal Amount { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Note { get; set; }
}

public record TransactionDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string Type,
    decimal Amount,
    DateTime Date,
    string? Note);

public record TransactionSummaryDto(
    decimal Income,
    decimal Expenses,
    decimal Net,
    int Count);
