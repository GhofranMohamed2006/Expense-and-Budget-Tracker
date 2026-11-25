namespace Pennywise.DAL.Entities;

public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? CategoryId { get; set; }          // optional: transfers have no category
    public int AccountId { get; set; }
    public int? TransferToAccountId { get; set; } // only for Type == Transfer
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionType Type { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties (ERD: records / classifies / contains)
    public User User { get; set; } = null!;
    public Category? Category { get; set; }
    public Account Account { get; set; } = null!;
    public Account? TransferToAccount { get; set; }
}
