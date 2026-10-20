using System;
using System.Collections.Generic;
using System.Text;

namespace Pennywise.Backend
{
    internal class Transaction
    {
        public int TransactionID { get; set; }
        public int UserID { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set;  }
    }
}
