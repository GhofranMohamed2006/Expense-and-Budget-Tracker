using System;
using System.Collections.Generic;
using System.Text;

namespace Pennywise.Backend
{
    internal class CategoryExpense
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalExpenses { get; set; }
    }
}
