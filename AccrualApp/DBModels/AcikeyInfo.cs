using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class AcikeyInfo
    {
        public int AcitransactionId { get; set; }
        public int AcicompanyId { get; set; }
        public int? AcicustomerId { get; set; }
        public int AcilineItemId { get; set; }
        public DateTime AcitransactionDate { get; set; }
        public double Aciamount { get; set; }
        public string TransactionType { get; set; }
        public string Memo { get; set; }
    }
}
