using System;
using System.Collections.Generic;
using System.Text;

namespace Pennywise.Backend
{
    internal class MonthlyExpense
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalExpenses { get; set; }
    }
}
