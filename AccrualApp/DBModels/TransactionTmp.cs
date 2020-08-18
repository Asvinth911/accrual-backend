using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class TransactionTmp
    {
        public string Type { get; set; }
        public string AccountId { get; set; }
        public string CustomerId { get; set; }
        public string RegionId { get; set; }
        public int? Qty { get; set; }
        public double? SalesPrice { get; set; }
        public double? Amount { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? TransactionDateH { get; set; }
        public string Memo { get; set; }
        public string MemoH { get; set; }
        public string VendorId { get; set; }
        public string VendorIdH { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? DueDateH { get; set; }
        public bool? IsPaid { get; set; }
        public bool? IsPaidH { get; set; }
    }
}
