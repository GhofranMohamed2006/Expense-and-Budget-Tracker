using System;
using System.Collections.Generic;
using System.Text;

namespace Pennywise.Backend
{
    internal class Budget
    {
        public int BudgetID { get; set; }
        public int UserID { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public DateTime BudgetMonth { get; set; }
    }
}
