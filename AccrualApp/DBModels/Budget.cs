using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class Budget
    {
        public int Id { get; set; }
        public string AccountId { get; set; }
        public string RegionId { get; set; }
        public string CustomerId { get; set; }
        public DateTime Month { get; set; }
        public double BudgetAmount { get; set; }

        public virtual Account Account { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Region Region { get; set; }
    }
}
