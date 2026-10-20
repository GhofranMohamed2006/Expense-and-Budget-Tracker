using System;
using System.Collections.Generic;
using System.Text;

namespace Pennywise.Backend
{
    internal class BudgetSummary
    {
        public decimal BudgetAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal UsagePercentage { get; set; }
    }
}
