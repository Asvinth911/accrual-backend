using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class IpaData
    {
        public int? TransactionNumber { get; set; }
        public string TransactionType { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string CustomerName { get; set; }
        public string Memo { get; set; }
        public string Account { get; set; }
        public string Split { get; set; }
        public int? Qty { get; set; }
        public double? SalesPrice { get; set; }
        public double? Debit { get; set; }
        public double? Credit { get; set; }
        public double? Amount { get; set; }
        public string AccountType { get; set; }
    }
}
