using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class Projection
    {
        public int Id { get; set; }
        public string RegionId { get; set; }
        public string AccountId { get; set; }
        public double? Amount { get; set; }
        public DateTime? TransactionDate { get; set; }

        public virtual Account Account { get; set; }
        public virtual Region Region { get; set; }
    }
}
