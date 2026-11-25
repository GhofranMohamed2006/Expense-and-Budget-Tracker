namespace Pennywise.DAL.Entities;

public class Account
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties (ERD: owns / contains)
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    // Transfers that point to this account (Transaction.TransferToAccountId)
    public ICollection<Transaction> IncomingTransfers { get; set; } = new List<Transaction>();
}
